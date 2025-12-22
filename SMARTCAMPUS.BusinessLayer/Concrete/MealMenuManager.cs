using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Context;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class MealMenuManager : IMealMenuService
    {
        private readonly CampusContext _context;

        public MealMenuManager(CampusContext context)
        {
            _context = context;
        }

        public async Task<Response<List<MealMenuListDto>>> GetMenusAsync(DateTime? date = null, int? cafeteriaId = null, MealType? mealType = null)
        {
            var query = _context.MealMenus
                .Include(m => m.Cafeteria)
                .Include(m => m.MenuItems)
                .Where(m => m.IsActive && m.IsPublished)
                .AsQueryable();

            if (date.HasValue)
                query = query.Where(m => m.Date.Date == date.Value.Date);

            if (cafeteriaId.HasValue)
                query = query.Where(m => m.CafeteriaId == cafeteriaId.Value);

            if (mealType.HasValue)
                query = query.Where(m => m.MealType == mealType.Value);

            var menus = await query
                .OrderByDescending(m => m.Date)
                .ThenBy(m => m.MealType)
                .Select(m => new MealMenuListDto
                {
                    Id = m.Id,
                    CafeteriaId = m.CafeteriaId,
                    CafeteriaName = m.Cafeteria.Name,
                    Date = m.Date,
                    MealType = m.MealType,
                    Price = m.Price,
                    IsPublished = m.IsPublished,
                    ItemCount = m.MenuItems.Count
                })
                .ToListAsync();

            return Response<List<MealMenuListDto>>.Success(menus, 200);
        }

        public async Task<Response<MealMenuDto>> GetMenuByIdAsync(int id)
        {
            var menu = await _context.MealMenus
                .Include(m => m.Cafeteria)
                .Include(m => m.Nutrition)
                .Include(m => m.MenuItems)
                    .ThenInclude(mi => mi.FoodItem)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null)
                return Response<MealMenuDto>.Fail("Menü bulunamadı", 404);

            var dto = new MealMenuDto
            {
                Id = menu.Id,
                CafeteriaId = menu.CafeteriaId,
                CafeteriaName = menu.Cafeteria.Name,
                Date = menu.Date,
                MealType = menu.MealType,
                Price = menu.Price,
                IsPublished = menu.IsPublished,
                MenuItems = menu.MenuItems
                    .OrderBy(mi => mi.OrderIndex)
                    .Select(mi => new MealMenuItemDto
                    {
                        Id = mi.Id,
                        FoodItemId = mi.FoodItemId,
                        FoodItemName = mi.FoodItem.Name,
                        Category = mi.FoodItem.Category,
                        Calories = mi.FoodItem.Calories,
                        OrderIndex = mi.OrderIndex
                    }).ToList(),
                Nutrition = menu.Nutrition != null ? new MealNutritionDto
                {
                    Calories = menu.Nutrition.Calories,
                    Protein = menu.Nutrition.Protein,
                    Carbohydrates = menu.Nutrition.Carbohydrates,
                    Fat = menu.Nutrition.Fat,
                    Fiber = menu.Nutrition.Fiber,
                    Sodium = menu.Nutrition.Sodium
                } : null
            };

            return Response<MealMenuDto>.Success(dto, 200);
        }

        public async Task<Response<MealMenuDto>> CreateMenuAsync(MealMenuCreateDto dto)
        {
            // Aynı yemekhane, tarih ve öğün için menü var mı?
            var exists = await _context.MealMenus.AnyAsync(m =>
                m.CafeteriaId == dto.CafeteriaId &&
                m.Date.Date == dto.Date.Date &&
                m.MealType == dto.MealType &&
                m.IsActive);

            if (exists)
                return Response<MealMenuDto>.Fail("Bu yemekhane için bu tarih ve öğünde zaten bir menü mevcut", 400);

            // Yemekhane kontrolü
            var cafeteria = await _context.Cafeterias.FindAsync(dto.CafeteriaId);
            if (cafeteria == null || !cafeteria.IsActive)
                return Response<MealMenuDto>.Fail("Geçersiz yemekhane", 400);

            // FoodItem kontrolü
            if (dto.FoodItemIds.Any())
            {
                var validFoodItemCount = await _context.FoodItems
                    .CountAsync(f => dto.FoodItemIds.Contains(f.Id) && f.IsActive);
                if (validFoodItemCount != dto.FoodItemIds.Count)
                    return Response<MealMenuDto>.Fail("Geçersiz yemek içeriği ID'leri", 400);
            }

            var menu = new MealMenu
            {
                CafeteriaId = dto.CafeteriaId,
                Date = dto.Date.Date,
                MealType = dto.MealType,
                Price = dto.Price,
                IsPublished = dto.IsPublished,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _context.MealMenus.AddAsync(menu);
            await _context.SaveChangesAsync();

            // Menu Items ekle
            var orderIndex = 0;
            foreach (var foodItemId in dto.FoodItemIds)
            {
                var menuItem = new MealMenuItem
                {
                    MenuId = menu.Id,
                    FoodItemId = foodItemId,
                    OrderIndex = orderIndex++,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                await _context.MealMenuItems.AddAsync(menuItem);
            }

            // Nutrition ekle
            if (dto.Nutrition != null)
            {
                var nutrition = new MealNutrition
                {
                    MenuId = menu.Id,
                    Calories = dto.Nutrition.Calories,
                    Protein = dto.Nutrition.Protein,
                    Carbohydrates = dto.Nutrition.Carbohydrates,
                    Fat = dto.Nutrition.Fat,
                    Fiber = dto.Nutrition.Fiber,
                    Sodium = dto.Nutrition.Sodium,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                await _context.MealNutritions.AddAsync(nutrition);
            }

            await _context.SaveChangesAsync();

            return await GetMenuByIdAsync(menu.Id);
        }

        public async Task<Response<MealMenuDto>> UpdateMenuAsync(int id, MealMenuCreateDto dto)
        {
            var menu = await _context.MealMenus
                .Include(m => m.MenuItems)
                .Include(m => m.Nutrition)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null)
                return Response<MealMenuDto>.Fail("Menü bulunamadı", 404);

            // Farklı tarih/öğün kontrolü
            if (menu.Date.Date != dto.Date.Date || menu.MealType != dto.MealType || menu.CafeteriaId != dto.CafeteriaId)
            {
                var exists = await _context.MealMenus.AnyAsync(m =>
                    m.Id != id &&
                    m.CafeteriaId == dto.CafeteriaId &&
                    m.Date.Date == dto.Date.Date &&
                    m.MealType == dto.MealType &&
                    m.IsActive);

                if (exists)
                    return Response<MealMenuDto>.Fail("Bu yemekhane için bu tarih ve öğünde zaten bir menü mevcut", 400);
            }

            menu.CafeteriaId = dto.CafeteriaId;
            menu.Date = dto.Date.Date;
            menu.MealType = dto.MealType;
            menu.Price = dto.Price;
            menu.IsPublished = dto.IsPublished;
            menu.UpdatedDate = DateTime.UtcNow;

            // Mevcut menu items'ları sil ve yeniden ekle
            _context.MealMenuItems.RemoveRange(menu.MenuItems);

            var orderIndex = 0;
            foreach (var foodItemId in dto.FoodItemIds)
            {
                var menuItem = new MealMenuItem
                {
                    MenuId = menu.Id,
                    FoodItemId = foodItemId,
                    OrderIndex = orderIndex++,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                await _context.MealMenuItems.AddAsync(menuItem);
            }

            // Nutrition güncelle
            if (dto.Nutrition != null)
            {
                if (menu.Nutrition != null)
                {
                    menu.Nutrition.Calories = dto.Nutrition.Calories;
                    menu.Nutrition.Protein = dto.Nutrition.Protein;
                    menu.Nutrition.Carbohydrates = dto.Nutrition.Carbohydrates;
                    menu.Nutrition.Fat = dto.Nutrition.Fat;
                    menu.Nutrition.Fiber = dto.Nutrition.Fiber;
                    menu.Nutrition.Sodium = dto.Nutrition.Sodium;
                    menu.Nutrition.UpdatedDate = DateTime.UtcNow;
                }
                else
                {
                    var nutrition = new MealNutrition
                    {
                        MenuId = menu.Id,
                        Calories = dto.Nutrition.Calories,
                        Protein = dto.Nutrition.Protein,
                        Carbohydrates = dto.Nutrition.Carbohydrates,
                        Fat = dto.Nutrition.Fat,
                        Fiber = dto.Nutrition.Fiber,
                        Sodium = dto.Nutrition.Sodium,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _context.MealNutritions.AddAsync(nutrition);
                }
            }

            await _context.SaveChangesAsync();
            return await GetMenuByIdAsync(menu.Id);
        }

        public async Task<Response<NoDataDto>> PublishMenuAsync(int id)
        {
            var menu = await _context.MealMenus.FindAsync(id);
            if (menu == null)
                return Response<NoDataDto>.Fail("Menü bulunamadı", 404);

            if (menu.IsPublished)
                return Response<NoDataDto>.Fail("Menü zaten yayında", 400);

            menu.IsPublished = true;
            menu.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> UnpublishMenuAsync(int id)
        {
            var menu = await _context.MealMenus.FindAsync(id);
            if (menu == null)
                return Response<NoDataDto>.Fail("Menü bulunamadı", 404);

            if (!menu.IsPublished)
                return Response<NoDataDto>.Fail("Menü zaten yayında değil", 400);

            // Aktif rezervasyon kontrolü
            var hasActiveReservations = await _context.MealReservations
                .AnyAsync(r => r.MenuId == id && r.Status == MealReservationStatus.Reserved);
            if (hasActiveReservations)
                return Response<NoDataDto>.Fail("Bu menü için aktif rezervasyonlar var, yayından kaldırılamaz", 400);

            menu.IsPublished = false;
            menu.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> DeleteMenuAsync(int id, bool force = false)
        {
            var menu = await _context.MealMenus.FindAsync(id);
            if (menu == null)
                return Response<NoDataDto>.Fail("Menü bulunamadı", 404);

            // Aktif rezervasyon kontrolü
            var hasActiveReservations = await _context.MealReservations
                .AnyAsync(r => r.MenuId == id && r.Status == MealReservationStatus.Reserved);

            if (hasActiveReservations && !force)
                return Response<NoDataDto>.Fail("Bu menü için aktif rezervasyonlar var. Silmek için force=true kullanın.", 400);

            // Soft delete
            menu.IsActive = false;
            menu.IsPublished = false;
            menu.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> AddFoodItemToMenuAsync(int menuId, int foodItemId)
        {
            var menu = await _context.MealMenus.FindAsync(menuId);
            if (menu == null)
                return Response<NoDataDto>.Fail("Menü bulunamadı", 404);

            var foodItem = await _context.FoodItems.FindAsync(foodItemId);
            if (foodItem == null || !foodItem.IsActive)
                return Response<NoDataDto>.Fail("Yemek içeriği bulunamadı", 404);

            var exists = await _context.MealMenuItems.AnyAsync(m => m.MenuId == menuId && m.FoodItemId == foodItemId);
            if (exists)
                return Response<NoDataDto>.Fail("Bu yemek içeriği zaten menüde mevcut", 400);

            var maxOrder = await _context.MealMenuItems
                .Where(m => m.MenuId == menuId)
                .MaxAsync(m => (int?)m.OrderIndex) ?? -1;

            var menuItem = new MealMenuItem
            {
                MenuId = menuId,
                FoodItemId = foodItemId,
                OrderIndex = maxOrder + 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _context.MealMenuItems.AddAsync(menuItem);
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(201);
        }

        public async Task<Response<NoDataDto>> RemoveFoodItemFromMenuAsync(int menuId, int foodItemId)
        {
            var menuItem = await _context.MealMenuItems
                .FirstOrDefaultAsync(m => m.MenuId == menuId && m.FoodItemId == foodItemId);

            if (menuItem == null)
                return Response<NoDataDto>.Fail("Menü öğesi bulunamadı", 404);

            _context.MealMenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
