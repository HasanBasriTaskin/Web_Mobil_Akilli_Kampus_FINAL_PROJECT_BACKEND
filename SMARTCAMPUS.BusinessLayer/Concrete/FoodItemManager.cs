using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.FoodItem;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class FoodItemManager : IFoodItemService
    {
        private readonly CampusContext _context;

        public FoodItemManager(CampusContext context)
        {
            _context = context;
        }

        public async Task<Response<List<FoodItemDto>>> GetAllAsync(bool includeInactive = false)
        {
            var query = _context.FoodItems.AsQueryable();

            if (!includeInactive)
                query = query.Where(f => f.IsActive);

            var foodItems = await query
                .Select(f => new FoodItemDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Category = f.Category,
                    Calories = f.Calories,
                    IsActive = f.IsActive
                })
                .ToListAsync();

            return Response<List<FoodItemDto>>.Success(foodItems, 200);
        }

        public async Task<Response<List<FoodItemDto>>> GetByCategoryAsync(MealItemCategory category)
        {
            var foodItems = await _context.FoodItems
                .Where(f => f.IsActive && f.Category == category)
                .Select(f => new FoodItemDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Description = f.Description,
                    Category = f.Category,
                    Calories = f.Calories,
                    IsActive = f.IsActive
                })
                .ToListAsync();

            return Response<List<FoodItemDto>>.Success(foodItems, 200);
        }

        public async Task<Response<FoodItemDto>> GetByIdAsync(int id)
        {
            var foodItem = await _context.FoodItems.FindAsync(id);
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
            // Check if name already exists
            var exists = await _context.FoodItems.AnyAsync(f => f.Name == dto.Name);
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

            await _context.FoodItems.AddAsync(foodItem);
            await _context.SaveChangesAsync();

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
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
                return Response<FoodItemDto>.Fail("Yemek içeriği bulunamadı", 404);

            // Check if new name conflicts with another food item
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != foodItem.Name)
            {
                var nameExists = await _context.FoodItems.AnyAsync(f => f.Name == dto.Name && f.Id != id);
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
            await _context.SaveChangesAsync();

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
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
                return Response<NoDataDto>.Fail("Yemek içeriği bulunamadı", 404);

            // Bağımlılık kontrolü: Aktif menüde kullanılıyor mu?
            var isUsedInActiveMenu = await _context.MealMenuItems
                .AnyAsync(m => m.FoodItemId == id && m.Menu.IsActive);
            if (isUsedInActiveMenu)
                return Response<NoDataDto>.Fail("Bu yemek içeriği aktif menülerde kullanılıyor, silinemez", 400);

            // Soft delete
            foodItem.IsActive = false;
            foodItem.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
