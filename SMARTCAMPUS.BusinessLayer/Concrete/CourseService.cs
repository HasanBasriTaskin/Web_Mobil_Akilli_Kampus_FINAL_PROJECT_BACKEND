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
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CampusContext _context;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper, CampusContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Response<PagedResponse<CourseDto>>> GetCoursesAsync(CourseQueryParameters queryParams)
        {
            try
            {
                var coursesQuery = _context.Courses.Where(c => c.IsActive).Include(c => c.Department).AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(queryParams.Search))
                {
                    var searchLower = queryParams.Search.ToLower();
                    coursesQuery = coursesQuery.Where(c =>
                        c.Code.ToLower().Contains(searchLower) ||
                        c.Name.ToLower().Contains(searchLower) ||
                        (c.Description != null && c.Description.ToLower().Contains(searchLower)));
                }

                // Department filter
                if (queryParams.DepartmentId.HasValue)
                {
                    coursesQuery = coursesQuery.Where(c => c.DepartmentId == queryParams.DepartmentId.Value);
                }

                // Credits filter
                if (queryParams.MinCredits.HasValue)
                {
                    coursesQuery = coursesQuery.Where(c => c.Credits >= queryParams.MinCredits.Value);
                }

                if (queryParams.MaxCredits.HasValue)
                {
                    coursesQuery = coursesQuery.Where(c => c.Credits <= queryParams.MaxCredits.Value);
                }

                // Sorting
                if (!string.IsNullOrWhiteSpace(queryParams.SortBy))
                {
                    coursesQuery = queryParams.SortBy.ToLower() switch
                    {
                        "code" => queryParams.SortOrder?.ToLower() == "desc"
                            ? coursesQuery.OrderByDescending(c => c.Code)
                            : coursesQuery.OrderBy(c => c.Code),
                        "name" => queryParams.SortOrder?.ToLower() == "desc"
                            ? coursesQuery.OrderByDescending(c => c.Name)
                            : coursesQuery.OrderBy(c => c.Name),
                        "credits" => queryParams.SortOrder?.ToLower() == "desc"
                            ? coursesQuery.OrderByDescending(c => c.Credits)
                            : coursesQuery.OrderBy(c => c.Credits),
                        _ => coursesQuery.OrderBy(c => c.Code)
                    };
                }
                else
                {
                    coursesQuery = coursesQuery.OrderBy(c => c.Code);
                }

                // Get total count before pagination
                var totalRecords = await coursesQuery.CountAsync();

                // Pagination
                var courses = await coursesQuery
                    .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                    .Take(queryParams.PageSize)
                    .ToListAsync();

                var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
                var pagedResponse = new PagedResponse<CourseDto>(
                    courseDtos,
                    queryParams.PageNumber,
                    queryParams.PageSize,
                    totalRecords
                );

                return Response<PagedResponse<CourseDto>>.Success(pagedResponse, 200);
            }
            catch (Exception ex)
            {
                return Response<PagedResponse<CourseDto>>.Fail($"Error retrieving courses: {ex.Message}", 500);
            }
        }

        public async Task<Response<CourseDto>> GetCourseByIdAsync(int courseId)
        {
            try
            {
                var course = await _unitOfWork.Courses.GetCourseWithPrerequisitesAsync(courseId);
                if (course == null)
                    return Response<CourseDto>.Fail("Course not found", 404);

                var courseDto = _mapper.Map<CourseDto>(course);
                if (course.Prerequisites != null)
                {
                    courseDto.Prerequisites = course.Prerequisites
                        .Select(p => p.PrerequisiteCourse.Code)
                        .ToList();
                }

                return Response<CourseDto>.Success(courseDto, 200);
            }
            catch (Exception ex)
            {
                return Response<CourseDto>.Fail($"Error retrieving course: {ex.Message}", 500);
            }
        }

        public async Task<Response<CourseDto>> GetCourseByCodeAsync(string code)
        {
            try
            {
                var course = await _unitOfWork.Courses.GetCourseByCodeAsync(code);
                if (course == null)
                    return Response<CourseDto>.Fail("Course not found", 404);

                var courseDto = _mapper.Map<CourseDto>(course);
                return Response<CourseDto>.Success(courseDto, 200);
            }
            catch (Exception ex)
            {
                return Response<CourseDto>.Fail($"Error retrieving course: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<CourseDto>>> GetCoursesByDepartmentAsync(int departmentId)
        {
            try
            {
                var courses = await _unitOfWork.Courses.GetCoursesByDepartmentAsync(departmentId);
                var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
                return Response<IEnumerable<CourseDto>>.Success(courseDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<CourseDto>>.Fail($"Error retrieving courses: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<CourseSectionDto>>> GetCourseSectionsAsync(int courseId)
        {
            try
            {
                var sections = await _unitOfWork.CourseSections.GetSectionsByCourseAsync(courseId);
                var sectionDtos = _mapper.Map<IEnumerable<CourseSectionDto>>(sections);
                return Response<IEnumerable<CourseSectionDto>>.Success(sectionDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<CourseSectionDto>>.Fail($"Error retrieving sections: {ex.Message}", 500);
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

        public async Task<Response<CourseDto>> CreateCourseAsync(CourseCreateDto courseCreateDto)
        {
            try
            {
                // Check if course code already exists
                var existingCourse = await _unitOfWork.Courses.GetCourseByCodeAsync(courseCreateDto.Code);
                if (existingCourse != null)
                    return Response<CourseDto>.Fail("Course code already exists", 400);

                // Verify department exists
                var department = await _unitOfWork.Departments.GetByIdAsync(courseCreateDto.DepartmentId);
                if (department == null)
                    return Response<CourseDto>.Fail("Department not found", 404);

                var course = _mapper.Map<SMARTCAMPUS.EntityLayer.Models.Course>(courseCreateDto);
                await _unitOfWork.Courses.AddAsync(course);
                await _unitOfWork.CommitAsync();

                // Add prerequisites if provided
                if (courseCreateDto.PrerequisiteCodes != null && courseCreateDto.PrerequisiteCodes.Any())
                {
                    var prerequisites = new List<CoursePrerequisite>();
                    foreach (var prereqCode in courseCreateDto.PrerequisiteCodes)
                    {
                        var prereqCourse = await _unitOfWork.Courses.GetCourseByCodeAsync(prereqCode);
                        if (prereqCourse != null && prereqCourse.Id != course.Id) // Prevent self-reference
                        {
                            prerequisites.Add(new CoursePrerequisite
                            {
                                CourseId = course.Id,
                                PrerequisiteCourseId = prereqCourse.Id
                            });
                        }
                    }

                    if (prerequisites.Any())
                    {
                        await _context.CoursePrerequisites.AddRangeAsync(prerequisites);
                        await _context.SaveChangesAsync();
                    }
                }

                var courseDto = await GetCourseByIdAsync(course.Id);
                return courseDto;
            }
            catch (Exception ex)
            {
                return Response<CourseDto>.Fail($"Error creating course: {ex.Message}", 500);
            }
        }

        public async Task<Response<CourseDto>> UpdateCourseAsync(int courseId, CourseUpdateDto courseUpdateDto)
        {
            try
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                    return Response<CourseDto>.Fail("Course not found", 404);

                // Update properties if provided
                if (!string.IsNullOrWhiteSpace(courseUpdateDto.Name))
                    course.Name = courseUpdateDto.Name;

                if (courseUpdateDto.Description != null)
                    course.Description = courseUpdateDto.Description;

                if (courseUpdateDto.Credits.HasValue)
                    course.Credits = courseUpdateDto.Credits.Value;

                if (courseUpdateDto.ECTS.HasValue)
                    course.ECTS = courseUpdateDto.ECTS.Value;

                if (courseUpdateDto.SyllabusUrl != null)
                    course.SyllabusUrl = courseUpdateDto.SyllabusUrl;

                if (courseUpdateDto.DepartmentId.HasValue)
                {
                    var department = await _unitOfWork.Departments.GetByIdAsync(courseUpdateDto.DepartmentId.Value);
                    if (department == null)
                        return Response<CourseDto>.Fail("Department not found", 404);
                    course.DepartmentId = courseUpdateDto.DepartmentId.Value;
                }

                course.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Courses.Update(course);
                await _unitOfWork.CommitAsync();

                // Update prerequisites if provided
                if (courseUpdateDto.PrerequisiteCodes != null)
                {
                    // Remove existing prerequisites
                    var existingPrereqs = await _context.CoursePrerequisites
                        .Where(p => p.CourseId == courseId && p.IsActive)
                        .ToListAsync();
                    if (existingPrereqs.Any())
                    {
                        _context.CoursePrerequisites.RemoveRange(existingPrereqs);
                    }

                    // Add new prerequisites
                    if (courseUpdateDto.PrerequisiteCodes.Any())
                    {
                        var prerequisites = new List<CoursePrerequisite>();
                        foreach (var prereqCode in courseUpdateDto.PrerequisiteCodes)
                        {
                            var prereqCourse = await _unitOfWork.Courses.GetCourseByCodeAsync(prereqCode);
                            if (prereqCourse != null && prereqCourse.Id != courseId) // Prevent self-reference
                            {
                                prerequisites.Add(new CoursePrerequisite
                                {
                                    CourseId = courseId,
                                    PrerequisiteCourseId = prereqCourse.Id
                                });
                            }
                        }

                        if (prerequisites.Any())
                        {
                            await _context.CoursePrerequisites.AddRangeAsync(prerequisites);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                var courseDto = await GetCourseByIdAsync(courseId);
                return courseDto;
            }
            catch (Exception ex)
            {
                return Response<CourseDto>.Fail($"Error updating course: {ex.Message}", 500);
            }
        }

        public async Task<Response<NoDataDto>> DeleteCourseAsync(int courseId)
        {
            try
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(courseId);
                if (course == null)
                    return Response<NoDataDto>.Fail("Course not found", 404);

                // Soft delete
                course.IsActive = false;
                course.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Courses.Update(course);
                await _unitOfWork.CommitAsync();

                return Response<NoDataDto>.Success(200);
            }
            catch (Exception ex)
            {
                return Response<NoDataDto>.Fail($"Error deleting course: {ex.Message}", 500);
            }
        }
    }
}



