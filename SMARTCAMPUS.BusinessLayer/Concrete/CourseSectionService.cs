using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class CourseSectionService : ICourseSectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CampusContext _context;

        public CourseSectionService(IUnitOfWork unitOfWork, IMapper mapper, CampusContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Response<PagedResponse<CourseSectionDto>>> GetSectionsAsync(CourseSectionQueryParameters queryParams)
        {
            try
            {
                var sectionsQuery = _context.CourseSections
                    .Where(s => s.IsActive)
                    .Include(s => s.Course)
                        .ThenInclude(c => c.Department)
                    .Include(s => s.Instructor)
                    .Include(s => s.Classroom)
                    .AsQueryable();

                // Course filter
                if (queryParams.CourseId.HasValue)
                {
                    sectionsQuery = sectionsQuery.Where(s => s.CourseId == queryParams.CourseId.Value);
                }

                // Semester filter
                if (!string.IsNullOrWhiteSpace(queryParams.Semester))
                {
                    sectionsQuery = sectionsQuery.Where(s => s.Semester == queryParams.Semester);
                }

                // Year filter
                if (queryParams.Year.HasValue)
                {
                    sectionsQuery = sectionsQuery.Where(s => s.Year == queryParams.Year.Value);
                }

                // Instructor filter
                if (!string.IsNullOrWhiteSpace(queryParams.InstructorId))
                {
                    sectionsQuery = sectionsQuery.Where(s => s.InstructorId == queryParams.InstructorId);
                }

                // Search filter
                if (!string.IsNullOrWhiteSpace(queryParams.Search))
                {
                    var searchLower = queryParams.Search.ToLower();
                    sectionsQuery = sectionsQuery.Where(s =>
                        s.Course.Code.ToLower().Contains(searchLower) ||
                        s.Course.Name.ToLower().Contains(searchLower) ||
                        s.SectionNumber.ToLower().Contains(searchLower));
                }

                // Sorting
                if (!string.IsNullOrWhiteSpace(queryParams.SortBy))
                {
                    sectionsQuery = queryParams.SortBy.ToLower() switch
                    {
                        "coursecode" => queryParams.SortOrder?.ToLower() == "desc"
                            ? sectionsQuery.OrderByDescending(s => s.Course.Code)
                            : sectionsQuery.OrderBy(s => s.Course.Code),
                        "sectionnumber" => queryParams.SortOrder?.ToLower() == "desc"
                            ? sectionsQuery.OrderByDescending(s => s.SectionNumber)
                            : sectionsQuery.OrderBy(s => s.SectionNumber),
                        "semester" => queryParams.SortOrder?.ToLower() == "desc"
                            ? sectionsQuery.OrderByDescending(s => s.Semester)
                            : sectionsQuery.OrderBy(s => s.Semester),
                        _ => sectionsQuery.OrderBy(s => s.Course.Code).ThenBy(s => s.SectionNumber)
                    };
                }
                else
                {
                    sectionsQuery = sectionsQuery.OrderBy(s => s.Course.Code).ThenBy(s => s.SectionNumber);
                }

                // Get total count before pagination
                var totalRecords = await sectionsQuery.CountAsync();

                // Pagination
                var sections = await sectionsQuery
                    .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .ToListAsync();

                var sectionDtos = _mapper.Map<IEnumerable<CourseSectionDto>>(sections);
                foreach (var dto in sectionDtos)
                {
                    var section = sections.FirstOrDefault(s => s.Id == dto.Id);
                    if (section != null)
                    {
                        dto.CourseCode = section.Course?.Code;
                        dto.CourseName = section.Course?.Name;
                        dto.InstructorName = section.Instructor?.FullName;
                        dto.ClassroomInfo = section.Classroom != null
                            ? $"{section.Classroom.Building}-{section.Classroom.RoomNumber}"
                            : null;
                    }
                }

                var pagedResponse = new PagedResponse<CourseSectionDto>(
                    sectionDtos,
                    queryParams.PageNumber,
                    queryParams.PageSize,
                    totalRecords
                );

                return Response<PagedResponse<CourseSectionDto>>.Success(pagedResponse, 200);
            }
            catch (Exception ex)
            {
                return Response<PagedResponse<CourseSectionDto>>.Fail($"Error retrieving sections: {ex.Message}", 500);
            }
        }

        public async Task<Response<CourseSectionDto>> GetSectionByIdAsync(int sectionId)
        {
            try
            {
                var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(sectionId);
                if (section == null)
                    return Response<CourseSectionDto>.Fail("Section not found", 404);

                var sectionDto = _mapper.Map<CourseSectionDto>(section);
                sectionDto.CourseCode = section.Course?.Code;
                sectionDto.CourseName = section.Course?.Name;
                sectionDto.InstructorName = section.Instructor?.FullName;
                sectionDto.ClassroomInfo = section.Classroom != null
                    ? $"{section.Classroom.Building}-{section.Classroom.RoomNumber}"
                    : null;

                return Response<CourseSectionDto>.Success(sectionDto, 200);
            }
            catch (Exception ex)
            {
                return Response<CourseSectionDto>.Fail($"Error retrieving section: {ex.Message}", 500);
            }
        }

        public async Task<Response<CourseSectionDto>> CreateSectionAsync(CourseSectionCreateDto sectionCreateDto)
        {
            try
            {
                // Verify course exists
                var course = await _unitOfWork.Courses.GetByIdAsync(sectionCreateDto.CourseId);
                if (course == null)
                    return Response<CourseSectionDto>.Fail("Course not found", 404);

                // Check if section already exists (unique constraint: CourseId, SectionNumber, Semester, Year)
                var existingSection = await _context.CourseSections
                    .FirstOrDefaultAsync(s =>
                        s.CourseId == sectionCreateDto.CourseId &&
                        s.SectionNumber == sectionCreateDto.SectionNumber &&
                        s.Semester == sectionCreateDto.Semester &&
                        s.Year == sectionCreateDto.Year &&
                        s.IsActive);

                if (existingSection != null)
                    return Response<CourseSectionDto>.Fail("Section already exists for this course, semester, and year", 400);

                // Verify instructor exists if provided
                if (!string.IsNullOrEmpty(sectionCreateDto.InstructorId))
                {
                    var instructor = await _context.Users.FindAsync(sectionCreateDto.InstructorId);
                    if (instructor == null)
                        return Response<CourseSectionDto>.Fail("Instructor not found", 404);
                }

                // Verify classroom exists if provided
                if (sectionCreateDto.ClassroomId.HasValue)
                {
                    var classroom = await _unitOfWork.Classrooms.GetByIdAsync(sectionCreateDto.ClassroomId.Value);
                    if (classroom == null)
                        return Response<CourseSectionDto>.Fail("Classroom not found", 404);
                }

                var section = _mapper.Map<CourseSection>(sectionCreateDto);
                section.EnrolledCount = 0;

                await _unitOfWork.CourseSections.AddAsync(section);
                await _unitOfWork.CommitAsync();

                var sectionDto = await GetSectionByIdAsync(section.Id);
                return sectionDto;
            }
            catch (Exception ex)
            {
                return Response<CourseSectionDto>.Fail($"Error creating section: {ex.Message}", 500);
            }
        }

        public async Task<Response<CourseSectionDto>> UpdateSectionAsync(int sectionId, CourseSectionUpdateDto sectionUpdateDto)
        {
            try
            {
                var section = await _unitOfWork.CourseSections.GetSectionWithDetailsAsync(sectionId);
                if (section == null)
                    return Response<CourseSectionDto>.Fail("Section not found", 404);

                // Update properties if provided
                if (!string.IsNullOrWhiteSpace(sectionUpdateDto.SectionNumber))
                {
                    // Check uniqueness if section number is being changed
                    if (sectionUpdateDto.SectionNumber != section.SectionNumber)
                    {
                        var existingSection = await _context.CourseSections
                            .FirstOrDefaultAsync(s =>
                                s.CourseId == section.CourseId &&
                                s.SectionNumber == sectionUpdateDto.SectionNumber &&
                                s.Semester == (sectionUpdateDto.Semester ?? section.Semester) &&
                                s.Year == (sectionUpdateDto.Year ?? section.Year) &&
                                s.Id != sectionId &&
                                s.IsActive);

                        if (existingSection != null)
                            return Response<CourseSectionDto>.Fail("Section number already exists for this course, semester, and year", 400);
                    }
                    section.SectionNumber = sectionUpdateDto.SectionNumber;
                }

                if (!string.IsNullOrWhiteSpace(sectionUpdateDto.Semester))
                    section.Semester = sectionUpdateDto.Semester;

                if (sectionUpdateDto.Year.HasValue)
                    section.Year = sectionUpdateDto.Year.Value;

                if (sectionUpdateDto.InstructorId != null)
                {
                    if (string.IsNullOrEmpty(sectionUpdateDto.InstructorId))
                    {
                        section.InstructorId = null;
                    }
                    else
                    {
                        var instructor = await _context.Users.FindAsync(sectionUpdateDto.InstructorId);
                        if (instructor == null)
                            return Response<CourseSectionDto>.Fail("Instructor not found", 404);
                        section.InstructorId = sectionUpdateDto.InstructorId;
                    }
                }

                if (sectionUpdateDto.Capacity.HasValue)
                {
                    if (sectionUpdateDto.Capacity.Value < section.EnrolledCount)
                        return Response<CourseSectionDto>.Fail("Capacity cannot be less than enrolled count", 400);
                    section.Capacity = sectionUpdateDto.Capacity.Value;
                }

                if (sectionUpdateDto.ScheduleJson != null)
                    section.ScheduleJson = sectionUpdateDto.ScheduleJson;

                if (sectionUpdateDto.ClassroomId.HasValue)
                {
                    if (sectionUpdateDto.ClassroomId.Value == 0)
                    {
                        section.ClassroomId = null;
                    }
                    else
                    {
                        var classroom = await _unitOfWork.Classrooms.GetByIdAsync(sectionUpdateDto.ClassroomId.Value);
                        if (classroom == null)
                            return Response<CourseSectionDto>.Fail("Classroom not found", 404);
                        section.ClassroomId = sectionUpdateDto.ClassroomId.Value;
                    }
                }

                section.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.CourseSections.Update(section);
                await _unitOfWork.CommitAsync();

                var sectionDto = await GetSectionByIdAsync(sectionId);
                return sectionDto;
            }
            catch (Exception ex)
            {
                return Response<CourseSectionDto>.Fail($"Error updating section: {ex.Message}", 500);
            }
        }
    }
}

