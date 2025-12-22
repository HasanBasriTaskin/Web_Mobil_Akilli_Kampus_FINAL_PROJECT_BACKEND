using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class CafeteriaManager : ICafeteriaService
    {
        private readonly CampusContext _context;

        public CafeteriaManager(CampusContext context)
        {
            _context = context;
        }

        public async Task<Response<List<CafeteriaDto>>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.Cafeterias.AsQueryable();
            
            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            var cafeterias = await query
                .Select(c => new CafeteriaDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Location = c.Location,
                    Capacity = c.Capacity,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Response<List<CafeteriaDto>>.Success(cafeterias, 200);
        }

        public async Task<Response<CafeteriaDto>> GetByIdAsync(int id)
        {
            var cafeteria = await _context.Cafeterias.FindAsync(id);
            if (cafeteria == null)
                return Response<CafeteriaDto>.Fail("Yemekhane bulunamadı", 404);

            var dto = new CafeteriaDto
            {
                Id = cafeteria.Id,
                Name = cafeteria.Name,
                Location = cafeteria.Location,
                Capacity = cafeteria.Capacity,
                IsActive = cafeteria.IsActive
            };

            return Response<CafeteriaDto>.Success(dto, 200);
        }

        public async Task<Response<CafeteriaDto>> CreateAsync(CafeteriaCreateDto dto)
        {
            // Check if name already exists
            var exists = await _context.Cafeterias.AnyAsync(c => c.Name == dto.Name);
            if (exists)
                return Response<CafeteriaDto>.Fail("Bu isimde bir yemekhane zaten mevcut", 400);

            var cafeteria = new Cafeteria
            {
                Name = dto.Name,
                Location = dto.Location,
                Capacity = dto.Capacity,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _context.Cafeterias.AddAsync(cafeteria);
            await _context.SaveChangesAsync();

            var resultDto = new CafeteriaDto
            {
                Id = cafeteria.Id,
                Name = cafeteria.Name,
                Location = cafeteria.Location,
                Capacity = cafeteria.Capacity,
                IsActive = cafeteria.IsActive
            };

            return Response<CafeteriaDto>.Success(resultDto, 201);
        }

        public async Task<Response<CafeteriaDto>> UpdateAsync(int id, CafeteriaUpdateDto dto)
        {
            var cafeteria = await _context.Cafeterias.FindAsync(id);
            if (cafeteria == null)
                return Response<CafeteriaDto>.Fail("Yemekhane bulunamadı", 404);

            // Check if new name conflicts with another cafeteria
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != cafeteria.Name)
            {
                var nameExists = await _context.Cafeterias.AnyAsync(c => c.Name == dto.Name && c.Id != id);
                if (nameExists)
                    return Response<CafeteriaDto>.Fail("Bu isimde bir yemekhane zaten mevcut", 400);
                cafeteria.Name = dto.Name;
            }

            if (!string.IsNullOrEmpty(dto.Location))
                cafeteria.Location = dto.Location;

            if (dto.Capacity.HasValue)
                cafeteria.Capacity = dto.Capacity.Value;

            if (dto.IsActive.HasValue)
                cafeteria.IsActive = dto.IsActive.Value;

            cafeteria.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var resultDto = new CafeteriaDto
            {
                Id = cafeteria.Id,
                Name = cafeteria.Name,
                Location = cafeteria.Location,
                Capacity = cafeteria.Capacity,
                IsActive = cafeteria.IsActive
            };

            return Response<CafeteriaDto>.Success(resultDto, 200);
        }

        public async Task<Response<NoDataDto>> DeleteAsync(int id)
        {
            var cafeteria = await _context.Cafeterias.FindAsync(id);
            if (cafeteria == null)
                return Response<NoDataDto>.Fail("Yemekhane bulunamadı", 404);

            // Bağımlılık kontrolü: Aktif menü veya rezervasyon var mı?
            var hasActiveMenus = await _context.MealMenus.AnyAsync(m => m.CafeteriaId == id && m.IsActive);
            if (hasActiveMenus)
                return Response<NoDataDto>.Fail("Bu yemekhanenin aktif menüleri var, silinemez", 400);

            var hasActiveReservations = await _context.MealReservations
                .AnyAsync(r => r.CafeteriaId == id && r.Status == EntityLayer.Enums.MealReservationStatus.Reserved);
            if (hasActiveReservations)
                return Response<NoDataDto>.Fail("Bu yemekhanenin aktif rezervasyonları var, silinemez", 400);

            // Soft delete
            cafeteria.IsActive = false;
            cafeteria.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
