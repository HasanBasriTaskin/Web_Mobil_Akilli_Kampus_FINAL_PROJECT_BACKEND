using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class FoodItemManager : IFoodItemService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FoodItemManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<FoodItemDto>>> GetAllAsync(bool includeInactive = false)
        {
            var query = _unitOfWork.FoodItems.Where(f => includeInactive || f.IsActive);

            var foodItems = query
                .Select(f => new FoodItemDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Category = f.Category,
                    Calories = f.Calories,
                    IsActive = f.IsActive
                })
                .ToList();

            return Response<List<FoodItemDto>>.Success(foodItems, 200);
        }

        public async Task<Response<List<FoodItemDto>>> GetByCategoryAsync(MealItemCategory category)
        {
            var foodItems = await _unitOfWork.FoodItems.GetByCategoryAsync(category);

            var dtos = foodItems.Select(f => new FoodItemDto
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Category = f.Category,
                Calories = f.Calories,
                IsActive = f.IsActive
            }).ToList();

            return Response<List<FoodItemDto>>.Success(dtos, 200);
        }

        public async Task<Response<FoodItemDto>> GetByIdAsync(int id)
        {
            var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
            if (foodItem == null)
                return Response<FoodItemDto>.Fail("Yemek içeriği bulunamadı", 404);

            var dto = new FoodItemDto
            {
                Id = foodItem.Id,
                Name = foodItem.Name,
                Description = foodItem.Description,
                Category = foodItem.Category,
                Calories = foodItem.Calories,
                IsActive = foodItem.IsActive
            };

            return Response<FoodItemDto>.Success(dto, 200);
        }

        public async Task<Response<FoodItemDto>> CreateAsync(FoodItemCreateDto dto)
        {
            var exists = await _unitOfWork.FoodItems.NameExistsAsync(dto.Name);
            if (exists)
                return Response<FoodItemDto>.Fail("Bu isimde bir yemek içeriği zaten mevcut", 400);

            var foodItem = new FoodItem
            {
                Name = dto.Name,
                Description = dto.Description,
                Category = dto.Category,
                Calories = dto.Calories,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.FoodItems.AddAsync(foodItem);
            await _unitOfWork.CommitAsync();

            var resultDto = new FoodItemDto
            {
                Id = foodItem.Id,
                Name = foodItem.Name,
                Description = foodItem.Description,
                Category = foodItem.Category,
                Calories = foodItem.Calories,
                IsActive = foodItem.IsActive
            };

            return Response<FoodItemDto>.Success(resultDto, 201);
        }

        public async Task<Response<FoodItemDto>> UpdateAsync(int id, FoodItemUpdateDto dto)
        {
            var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
            if (foodItem == null)
                return Response<FoodItemDto>.Fail("Yemek içeriği bulunamadı", 404);

            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != foodItem.Name)
            {
                var nameExists = await _unitOfWork.FoodItems.NameExistsAsync(dto.Name, id);
                if (nameExists)
                    return Response<FoodItemDto>.Fail("Bu isimde bir yemek içeriği zaten mevcut", 400);
                foodItem.Name = dto.Name;
            }

            if (dto.Description != null)
                foodItem.Description = dto.Description;

            if (dto.Category.HasValue)
                foodItem.Category = dto.Category.Value;

            if (dto.Calories.HasValue)
                foodItem.Calories = dto.Calories.Value;

            if (dto.IsActive.HasValue)
                foodItem.IsActive = dto.IsActive.Value;

            foodItem.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.FoodItems.Update(foodItem);
            await _unitOfWork.CommitAsync();

            var resultDto = new FoodItemDto
            {
                Id = foodItem.Id,
                Name = foodItem.Name,
                Description = foodItem.Description,
                Category = foodItem.Category,
                Calories = foodItem.Calories,
                IsActive = foodItem.IsActive
            };

            return Response<FoodItemDto>.Success(resultDto, 200);
        }

        public async Task<Response<NoDataDto>> DeleteAsync(int id)
        {
            var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
            if (foodItem == null)
                return Response<NoDataDto>.Fail("Yemek içeriği bulunamadı", 404);

            var isUsedInActiveMenu = await _unitOfWork.FoodItems.IsUsedInActiveMenuAsync(id);
            if (isUsedInActiveMenu)
                return Response<NoDataDto>.Fail("Bu yemek içeriği aktif menülerde kullanılıyor, silinemez", 400);

            foodItem.IsActive = false;
            foodItem.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.FoodItems.Update(foodItem);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
