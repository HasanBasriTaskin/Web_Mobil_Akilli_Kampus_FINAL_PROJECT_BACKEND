using SMARTCAMPUS.BusinessLayer.Abstract;
using SMARTCAMPUS.BusinessLayer.Common;
using SMARTCAMPUS.DataAccessLayer.Abstract;
using SMARTCAMPUS.EntityLayer.DTOs;
using SMARTCAMPUS.EntityLayer.DTOs.Meal.Menu;
using SMARTCAMPUS.EntityLayer.Enums;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.BusinessLayer.Concrete
{
    public class MealMenuManager : IMealMenuService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MealMenuManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<MealMenuListDto>>> GetMenusAsync(DateTime? date = null, int? cafeteriaId = null, MealType? mealType = null)
        {
            var menus = await _unitOfWork.MealMenus.GetMenusAsync(date, cafeteriaId, mealType);

            var dtos = menus.Select(m => new MealMenuListDto
            {
                Id = m.Id,
                CafeteriaId = m.CafeteriaId,
                CafeteriaName = m.Cafeteria.Name,
                Date = m.Date,
                MealType = m.MealType,
                Price = m.Price,
                IsPublished = m.IsPublished,
                ItemCount = m.MenuItems.Count
            }).ToList();

            return Response<List<MealMenuListDto>>.Success(dtos, 200);
        }

        public async Task<Response<MealMenuDto>> GetMenuByIdAsync(int id)
        {
            var menu = await _unitOfWork.MealMenus.GetByIdWithDetailsAsync(id);
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
            var exists = await _unitOfWork.MealMenus.ExistsForCafeteriaDateMealTypeAsync(dto.CafeteriaId, dto.Date, dto.MealType);
            if (exists)
                return Response<MealMenuDto>.Fail("Bu yemekhane için bu tarih ve öğünde zaten bir menü mevcut", 400);

            var cafeteria = await _unitOfWork.Cafeterias.GetByIdAsync(dto.CafeteriaId);
            if (cafeteria == null || !cafeteria.IsActive)
                return Response<MealMenuDto>.Fail("Geçersiz yemekhane", 400);

            if (dto.FoodItemIds.Any())
            {
                var validFoodItems = _unitOfWork.FoodItems.Where(f => dto.FoodItemIds.Contains(f.Id) && f.IsActive).ToList();
                if (validFoodItems.Count != dto.FoodItemIds.Count)
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

            await _unitOfWork.MealMenus.AddAsync(menu);
            await _unitOfWork.CommitAsync();

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
                await _unitOfWork.MealMenuItems.AddAsync(menuItem);
            }

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
                await _unitOfWork.MealNutritions.AddAsync(nutrition);
            }

            await _unitOfWork.CommitAsync();
            return await GetMenuByIdAsync(menu.Id);
        }

        public async Task<Response<MealMenuDto>> UpdateMenuAsync(int id, MealMenuCreateDto dto)
        {
            var menu = await _unitOfWork.MealMenus.GetByIdWithDetailsAsync(id);
            if (menu == null)
                return Response<MealMenuDto>.Fail("Menü bulunamadı", 404);

            if (menu.Date.Date != dto.Date.Date || menu.MealType != dto.MealType || menu.CafeteriaId != dto.CafeteriaId)
            {
                var exists = await _unitOfWork.MealMenus.ExistsForCafeteriaDateMealTypeAsync(dto.CafeteriaId, dto.Date, dto.MealType, id);
                if (exists)
                    return Response<MealMenuDto>.Fail("Bu yemekhane için bu tarih ve öğünde zaten bir menü mevcut", 400);
            }

            menu.CafeteriaId = dto.CafeteriaId;
            menu.Date = dto.Date.Date;
            menu.MealType = dto.MealType;
            menu.Price = dto.Price;
            menu.IsPublished = dto.IsPublished;
            menu.UpdatedDate = DateTime.UtcNow;

            _unitOfWork.MealMenus.Update(menu);
            await _unitOfWork.MealMenuItems.RemoveByMenuIdAsync(id);

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
                await _unitOfWork.MealMenuItems.AddAsync(menuItem);
            }

            if (dto.Nutrition != null)
            {
                var existingNutrition = await _unitOfWork.MealNutritions.GetByMenuIdAsync(id);
                if (existingNutrition != null)
                {
                    existingNutrition.Calories = dto.Nutrition.Calories;
                    existingNutrition.Protein = dto.Nutrition.Protein;
                    existingNutrition.Carbohydrates = dto.Nutrition.Carbohydrates;
                    existingNutrition.Fat = dto.Nutrition.Fat;
                    existingNutrition.Fiber = dto.Nutrition.Fiber;
                    existingNutrition.Sodium = dto.Nutrition.Sodium;
                    existingNutrition.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.MealNutritions.Update(existingNutrition);
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
                    await _unitOfWork.MealNutritions.AddAsync(nutrition);
                }
            }

            await _unitOfWork.CommitAsync();
            return await GetMenuByIdAsync(menu.Id);
        }

        public async Task<Response<NoDataDto>> PublishMenuAsync(int id)
        {
            var menu = await _unitOfWork.MealMenus.GetByIdAsync(id);
            if (menu == null)
                return Response<NoDataDto>.Fail("Menü bulunamadı", 404);

            if (menu.IsPublished)
                return Response<NoDataDto>.Fail("Menü zaten yayında", 400);

            menu.IsPublished = true;
            menu.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.MealMenus.Update(menu);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> UnpublishMenuAsync(int id)
        {
            var menu = await _unitOfWork.MealMenus.GetByIdAsync(id);
            if (menu == null)
                return Response<NoDataDto>.Fail("Menü bulunamadı", 404);

            if (!menu.IsPublished)
                return Response<NoDataDto>.Fail("Menü zaten yayında değil", 400);

            var hasActiveReservations = await _unitOfWork.MealMenus.HasActiveReservationsAsync(id);
            if (hasActiveReservations)
                return Response<NoDataDto>.Fail("Bu menü için aktif rezervasyonlar var, yayından kaldırılamaz", 400);

            menu.IsPublished = false;
            menu.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.MealMenus.Update(menu);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> DeleteMenuAsync(int id, bool force = false)
        {
            var menu = await _unitOfWork.MealMenus.GetByIdAsync(id);
            if (menu == null)
                return Response<NoDataDto>.Fail("Menü bulunamadı", 404);

            var hasActiveReservations = await _unitOfWork.MealMenus.HasActiveReservationsAsync(id);
            if (hasActiveReservations && !force)
                return Response<NoDataDto>.Fail("Bu menü için aktif rezervasyonlar var. Silmek için force=true kullanın.", 400);

            menu.IsActive = false;
            menu.IsPublished = false;
            menu.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.MealMenus.Update(menu);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }

        public async Task<Response<NoDataDto>> AddFoodItemToMenuAsync(int menuId, int foodItemId)
        {
            var menu = await _unitOfWork.MealMenus.GetByIdAsync(menuId);
            if (menu == null)
                return Response<NoDataDto>.Fail("Menü bulunamadı", 404);

            var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(foodItemId);
            if (foodItem == null || !foodItem.IsActive)
                return Response<NoDataDto>.Fail("Yemek içeriği bulunamadı", 404);

            var exists = await _unitOfWork.MealMenuItems.ExistsAsync(menuId, foodItemId);
            if (exists)
                return Response<NoDataDto>.Fail("Bu yemek içeriği zaten menüde mevcut", 400);

            var maxOrder = await _unitOfWork.MealMenuItems.GetMaxOrderIndexAsync(menuId);

            var menuItem = new MealMenuItem
            {
                MenuId = menuId,
                FoodItemId = foodItemId,
                OrderIndex = maxOrder + 1,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.MealMenuItems.AddAsync(menuItem);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(201);
        }

        public async Task<Response<NoDataDto>> RemoveFoodItemFromMenuAsync(int menuId, int foodItemId)
        {
            var menuItem = await _unitOfWork.MealMenuItems.GetByMenuAndFoodItemAsync(menuId, foodItemId);
            if (menuItem == null)
                return Response<NoDataDto>.Fail("Menü öğesi bulunamadı", 404);

            _unitOfWork.MealMenuItems.Remove(menuItem);
            await _unitOfWork.CommitAsync();

            return Response<NoDataDto>.Success(200);
        }
    }
}
