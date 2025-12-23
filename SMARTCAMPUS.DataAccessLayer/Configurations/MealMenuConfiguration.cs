using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class MealMenuConfiguration : IEntityTypeConfiguration<MealMenu>
    {
        public void Configure(EntityTypeBuilder<MealMenu> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Date)
                .IsRequired();

            builder.Property(x => x.MealType)
                .HasConversion<string>();

            builder.Property(x => x.Price)
                .HasColumnType("decimal(10,2)");

            // Unique constraint: One menu per cafeteria per date per meal type
            builder.HasIndex(x => new { x.CafeteriaId, x.Date, x.MealType }).IsUnique();

            // Relationships
            builder.HasOne(x => x.Cafeteria)
                .WithMany(x => x.Menus)
                .HasForeignKey(x => x.CafeteriaId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-one with MealNutrition
            builder.HasOne(x => x.Nutrition)
                .WithOne(x => x.Menu)
                .HasForeignKey<MealNutrition>(x => x.MenuId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
