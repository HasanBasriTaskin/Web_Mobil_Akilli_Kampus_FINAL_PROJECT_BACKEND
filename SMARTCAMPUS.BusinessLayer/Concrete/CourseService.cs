using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CourseService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<CourseDto>>> GetCoursesAsync()
        {
            try
            {
                var courses = await _unitOfWork.Courses.GetAllAsync();
                var courseDtos = _mapper.Map<IEnumerable<CourseDto>>(courses);
                return Response<IEnumerable<CourseDto>>.Success(courseDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<CourseDto>>.Fail($"Error retrieving courses: {ex.Message}", 500);
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
    }
}

