using AutoMapper;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;
using SMARTCAMPUS.EntityLayer.DTOs.Auth;
using SMARTCAMPUS.EntityLayer.DTOs.User;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RegisterUserDto, User>();
            CreateMap<User, UserListDto>();
            CreateMap<UserUpdateDto, User>();

            // Academic Management Mappings
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name))
                .ForMember(dest => dest.DepartmentCode, opt => opt.MapFrom(src => src.Department.Code));

            CreateMap<CourseSection, CourseSectionDto>()
                .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Course.Code))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.Name))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null && src.Instructor.User != null ? src.Instructor.User.FullName : null))
                .ForMember(dest => dest.ClassroomInfo, opt => opt.Ignore());

            CreateMap<Enrollment, EnrollmentDto>()
                .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.Student.StudentNumber))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.User.FullName))
                .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Section.Course.Code))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Section.Course.Name))
                .ForMember(dest => dest.SectionNumber, opt => opt.MapFrom(src => src.Section.SectionNumber))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Section.Instructor != null && src.Section.Instructor.User != null ? src.Section.Instructor.User.FullName : null))
                .ForMember(dest => dest.ScheduleJson, opt => opt.Ignore())
                .ForMember(dest => dest.CanDrop, opt => opt.Ignore())
                .ForMember(dest => dest.DropReason, opt => opt.Ignore())
                .ForMember(dest => dest.AttendancePercentage, opt => opt.Ignore());

            CreateMap<AttendanceSession, AttendanceSessionDto>()
                .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Section.Course.Code))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Section.Course.Name))
                .ForMember(dest => dest.InstructorName, opt => opt.MapFrom(src => src.Instructor != null && src.Instructor.User != null ? src.Instructor.User.FullName : null));

            CreateMap<AttendanceRecord, AttendanceRecordDto>()
                .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.Student.StudentNumber))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.User.FullName));

            CreateMap<ExcuseRequest, ExcuseRequestDto>()
                .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.Student.StudentNumber))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.User.FullName))
                .ForMember(dest => dest.SessionDate, opt => opt.MapFrom(src => src.Session.Date))
                .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Session.Section.Course.Code));

            CreateMap<Classroom, ClassroomDto>();

            CreateMap<Enrollment, GradeDto>()
                .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.Student.StudentNumber))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.User.FullName))
                .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Section.Course.Code))
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Section.Course.Name));

            // Course Create/Update Mappings
            CreateMap<CourseCreateDto, Course>()
                .ForMember(dest => dest.Prerequisites, opt => opt.Ignore());
            CreateMap<CourseUpdateDto, Course>()
                .ForMember(dest => dest.Prerequisites, opt => opt.Ignore())
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Course Section Create/Update Mappings
            CreateMap<CourseSectionCreateDto, CourseSection>();
            CreateMap<CourseSectionUpdateDto, CourseSection>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Academic Calendar Mappings
            CreateMap<AcademicCalendar, AcademicCalendarDto>();

            // Announcement Mappings
            CreateMap<Announcement, AnnouncementDto>();

            // Department Mappings
            CreateMap<Department, DepartmentDto>();
        }
    }
}
