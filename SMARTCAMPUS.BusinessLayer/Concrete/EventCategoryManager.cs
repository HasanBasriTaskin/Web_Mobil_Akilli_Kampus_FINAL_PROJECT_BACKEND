using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Event;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class EventCategoryManager : IEventCategoryService
    {
        private readonly CampusContext _context;

        public EventCategoryManager(CampusContext context)
        {
            _context = context;
        }

        public async Task<Response<List<EventCategoryDto>>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.EventCategories.AsQueryable();

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            var categories = await query
                .OrderBy(c => c.Name)
                .Select(c => new EventCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IconName = c.IconName,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Response<List<EventCategoryDto>>.Success(categories, 200);
        }

        public async Task<Response<EventCategoryDto>> GetByIdAsync(int id)
        {
            var category = await _context.EventCategories.FindAsync(id);
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
            var exists = await _context.EventCategories.AnyAsync(c => c.Name == dto.Name);
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

            await _context.EventCategories.AddAsync(category);
            await _context.SaveChangesAsync();

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
            var category = await _context.EventCategories.FindAsync(id);
            if (category == null)
                return Response<EventCategoryDto>.Fail("Kategori bulunamadı", 404);

            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != category.Name)
            {
                var nameExists = await _context.EventCategories.AnyAsync(c => c.Name == dto.Name && c.Id != id);
                if (nameExists)
                    return Response<EventCategoryDto>.Fail("Bu isimde bir kategori zaten mevcut", 400);
                category.Name = dto.Name;
            }

            if (dto.Description != null) category.Description = dto.Description;
            if (dto.IconName != null) category.IconName = dto.IconName;

            category.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

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
            var category = await _context.EventCategories.FindAsync(id);
            if (category == null)
                return Response<NoDataDto>.Fail("Kategori bulunamadı", 404);

            // Bağımlılık kontrolü
            var hasEvents = await _context.Events.AnyAsync(e => e.CategoryId == id && e.IsActive);
            if (hasEvents)
                return Response<NoDataDto>.Fail("Bu kategoride aktif etkinlikler var, silinemez", 400);

            category.IsActive = false;
            category.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
