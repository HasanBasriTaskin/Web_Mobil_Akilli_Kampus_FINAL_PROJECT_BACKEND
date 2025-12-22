using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class EventCategoryManager : IEventCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EventCategoryManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<EventCategoryDto>>> GetAllAsync(bool includeInactive = false)
        {
            var query = _unitOfWork.EventCategories.Where(c => includeInactive || c.IsActive);

            var categories = query
                .OrderBy(c => c.Name)
                .Select(c => new EventCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IconName = c.IconName,
                    IsActive = c.IsActive
                })
                .ToList();

            return Response<List<EventCategoryDto>>.Success(categories, 200);
        }

        public async Task<Response<EventCategoryDto>> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.EventCategories.GetByIdAsync(id);
            if (category == null)
                return Response<EventCategoryDto>.Fail("Kategori bulunamadı", 404);

            return Response<EventCategoryDto>.Success(new EventCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IconName = category.IconName,
                IsActive = category.IsActive
            }, 200);
        }

        public async Task<Response<EventCategoryDto>> CreateAsync(EventCategoryCreateDto dto)
        {
            var exists = await _unitOfWork.EventCategories.NameExistsAsync(dto.Name);
            if (exists)
                return Response<EventCategoryDto>.Fail("Bu isimde bir kategori zaten mevcut", 400);

            var category = new EventCategory
            {
                Name = dto.Name,
                Description = dto.Description,
                IconName = dto.IconName,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.EventCategories.AddAsync(category);
            await _unitOfWork.CommitAsync();

            return Response<EventCategoryDto>.Success(new EventCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IconName = category.IconName,
                IsActive = category.IsActive
            }, 201);
        }

        public async Task<Response<EventCategoryDto>> UpdateAsync(int id, EventCategoryUpdateDto dto)
        {
            var category = await _unitOfWork.EventCategories.GetByIdAsync(id);
            if (category == null)
                return Response<EventCategoryDto>.Fail("Kategori bulunamadı", 404);

            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != category.Name)
            {
                var nameExists = await _unitOfWork.EventCategories.NameExistsAsync(dto.Name, id);
                if (nameExists)
                    return Response<EventCategoryDto>.Fail("Bu isimde bir kategori zaten mevcut", 400);
                category.Name = dto.Name;
            }

            if (dto.Description != null) category.Description = dto.Description;
            if (dto.IconName != null) category.IconName = dto.IconName;

            category.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.EventCategories.Update(category);
            await _unitOfWork.CommitAsync();

            return Response<EventCategoryDto>.Success(new EventCategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IconName = category.IconName,
                IsActive = category.IsActive
            }, 200);
        }

        public async Task<Response<NoDataDto>> DeleteAsync(int id)
        {
            var category = await _unitOfWork.EventCategories.GetByIdAsync(id);
            if (category == null)
                return Response<NoDataDto>.Fail("Kategori bulunamadı", 404);

            var hasEvents = await _unitOfWork.EventCategories.HasActiveEventsAsync(id);
            if (hasEvents)
                return Response<NoDataDto>.Fail("Bu kategoride aktif etkinlikler var, silinemez", 400);

            category.IsActive = false;
            category.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.EventCategories.Update(category);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
