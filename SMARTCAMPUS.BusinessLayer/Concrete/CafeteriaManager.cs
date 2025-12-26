using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Cafeteria;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class CafeteriaManager : ICafeteriaService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CafeteriaManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<CafeteriaDto>>> GetAllAsync(bool includeInactive = false)
        {
            var query = _unitOfWork.Cafeterias.Where(c => includeInactive || c.IsActive);

            var cafeterias = query
                .Select(c => new CafeteriaDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Location = c.Location,
                    Capacity = c.Capacity,
                    IsActive = c.IsActive
                })
                .ToList();

            return Response<List<CafeteriaDto>>.Success(cafeterias, 200);
        }

        public async Task<Response<CafeteriaDto>> GetByIdAsync(int id)
        {
            var cafeteria = await _unitOfWork.Cafeterias.GetByIdAsync(id);
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
            var exists = await _unitOfWork.Cafeterias.NameExistsAsync(dto.Name);
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

            await _unitOfWork.Cafeterias.AddAsync(cafeteria);
            await _unitOfWork.CommitAsync();

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
            var cafeteria = await _unitOfWork.Cafeterias.GetByIdAsync(id);
            if (cafeteria == null)
                return Response<CafeteriaDto>.Fail("Yemekhane bulunamadı", 404);

            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != cafeteria.Name)
            {
                var nameExists = await _unitOfWork.Cafeterias.NameExistsAsync(dto.Name, id);
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
            _unitOfWork.Cafeterias.Update(cafeteria);
            await _unitOfWork.CommitAsync();

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
            var cafeteria = await _unitOfWork.Cafeterias.GetByIdAsync(id);
            if (cafeteria == null)
                return Response<NoDataDto>.Fail("Yemekhane bulunamadı", 404);

            var hasActiveMenus = await _unitOfWork.Cafeterias.HasActiveMenusAsync(id);
            if (hasActiveMenus)
                return Response<NoDataDto>.Fail("Bu yemekhanenin aktif menüleri var, silinemez", 400);

            var hasActiveReservations = await _unitOfWork.Cafeterias.HasActiveReservationsAsync(id);
            if (hasActiveReservations)
                return Response<NoDataDto>.Fail("Bu yemekhanenin aktif rezervasyonları var, silinemez", 400);

            cafeteria.IsActive = false;
            cafeteria.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Cafeterias.Update(cafeteria);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
