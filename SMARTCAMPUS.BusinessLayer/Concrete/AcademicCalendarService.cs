using AutoMapper;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class AcademicCalendarService : IAcademicCalendarService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AcademicCalendarService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<AcademicCalendarDto>>> GetCalendarsAsync(int? year = null, string? semester = null)
        {
            try
            {
                var calendars = await _unitOfWork.AcademicCalendars.GetCalendarsByYearAndSemesterAsync(year, semester);
                var calendarDtos = _mapper.Map<IEnumerable<AcademicCalendarDto>>(calendars);
                return Response<IEnumerable<AcademicCalendarDto>>.Success(calendarDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<AcademicCalendarDto>>.Fail($"Error retrieving academic calendar: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<AcademicCalendarDto>>> GetCalendarsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var calendars = await _unitOfWork.AcademicCalendars.GetCalendarsByDateRangeAsync(startDate, endDate);
                var calendarDtos = _mapper.Map<IEnumerable<AcademicCalendarDto>>(calendars);
                return Response<IEnumerable<AcademicCalendarDto>>.Success(calendarDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<AcademicCalendarDto>>.Fail($"Error retrieving academic calendar: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<AcademicCalendarDto>>> GetUpcomingEventsAsync(int days = 30)
        {
            try
            {
                var calendars = await _unitOfWork.AcademicCalendars.GetUpcomingEventsAsync(days);
                var calendarDtos = _mapper.Map<IEnumerable<AcademicCalendarDto>>(calendars);
                return Response<IEnumerable<AcademicCalendarDto>>.Success(calendarDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<AcademicCalendarDto>>.Fail($"Error retrieving upcoming events: {ex.Message}", 500);
            }
        }
    }
}
