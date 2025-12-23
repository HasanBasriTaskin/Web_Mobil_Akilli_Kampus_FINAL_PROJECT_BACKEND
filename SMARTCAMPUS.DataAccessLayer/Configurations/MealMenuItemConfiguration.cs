using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class MealMenuItemConfiguration : IEntityTypeConfiguration<MealMenuItem>
    {
        public void Configure(EntityTypeBuilder<MealMenuItem> builder)
        {
            builder.HasKey(x => x.Id);

            // Unique constraint: One food item per menu (no duplicates)
            builder.HasIndex(x => new { x.MenuId, x.FoodItemId }).IsUnique();

            // Relationships
            builder.HasOne(x => x.Menu)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.FoodItem)
                .WithMany(x => x.MenuItems)
                .HasForeignKey(x => x.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
