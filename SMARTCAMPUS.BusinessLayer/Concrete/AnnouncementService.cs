using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Academic;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class AnnouncementService : IAnnouncementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CampusContext _context;

        public AnnouncementService(IUnitOfWork unitOfWork, IMapper mapper, CampusContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Response<IEnumerable<AnnouncementDto>>> GetAnnouncementsAsync(string? targetAudience = null, int? departmentId = null)
        {
            try
            {
                var announcements = await _unitOfWork.Announcements.GetActiveAnnouncementsAsync(targetAudience, departmentId);
                var announcementDtos = _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
                
                foreach (var dto in announcementDtos)
                {
                    var announcement = announcements.FirstOrDefault(a => a.Id == dto.Id);
                    if (announcement != null)
                    {
                        dto.DepartmentName = announcement.Department?.Name;
                        dto.CreatedByName = announcement.CreatedBy?.FullName;
                    }
                }

                return Response<IEnumerable<AnnouncementDto>>.Success(announcementDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<AnnouncementDto>>.Fail($"Error retrieving announcements: {ex.Message}", 500);
            }
        }

        public async Task<Response<IEnumerable<AnnouncementDto>>> GetImportantAnnouncementsAsync()
        {
            try
            {
                var announcements = await _unitOfWork.Announcements.GetImportantAnnouncementsAsync();
                var announcementDtos = _mapper.Map<IEnumerable<AnnouncementDto>>(announcements);
                
                foreach (var dto in announcementDtos)
                {
                    var announcement = announcements.FirstOrDefault(a => a.Id == dto.Id);
                    if (announcement != null)
                    {
                        dto.DepartmentName = announcement.Department?.Name;
                        dto.CreatedByName = announcement.CreatedBy?.FullName;
                    }
                }

                return Response<IEnumerable<AnnouncementDto>>.Success(announcementDtos, 200);
            }
            catch (Exception ex)
            {
                return Response<IEnumerable<AnnouncementDto>>.Fail($"Error retrieving important announcements: {ex.Message}", 500);
            }
        }

        public async Task<Response<AnnouncementDto>> GetAnnouncementByIdAsync(int id)
        {
            try
            {
                var announcement = await _unitOfWork.Announcements.GetByIdAsync(id);
                if (announcement == null || !announcement.IsActive)
                    return Response<AnnouncementDto>.Fail("Announcement not found", 404);

                var announcementDto = _mapper.Map<AnnouncementDto>(announcement);
                announcementDto.DepartmentName = announcement.Department?.Name;
                announcementDto.CreatedByName = announcement.CreatedBy?.FullName;

                // Increment view count
                await IncrementViewCountAsync(id);

                return Response<AnnouncementDto>.Success(announcementDto, 200);
            }
            catch (Exception ex)
            {
                return Response<AnnouncementDto>.Fail($"Error retrieving announcement: {ex.Message}", 500);
            }
        }

        public async Task<Response<NoDataDto>> IncrementViewCountAsync(int id)
        {
            try
            {
                var announcement = await _unitOfWork.Announcements.GetByIdAsync(id);
                if (announcement == null)
                    return Response<NoDataDto>.Fail("Announcement not found", 404);

                announcement.ViewCount++;
                announcement.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Announcements.Update(announcement);
                await _unitOfWork.CommitAsync();

                return Response<NoDataDto>.Success(200);
            }
            catch (Exception ex)
            {
                return Response<NoDataDto>.Fail($"Error incrementing view count: {ex.Message}", 500);
            }
        }
    }
}
