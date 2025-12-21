using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class MealNutritionConfiguration : IEntityTypeConfiguration<MealNutrition>
    {
        public void Configure(EntityTypeBuilder<MealNutrition> builder)
        {
            builder.HasKey(x => x.Id);

            // Unique constraint: One nutrition per menu
            builder.HasIndex(x => x.MenuId).IsUnique();
        }
    }
}
