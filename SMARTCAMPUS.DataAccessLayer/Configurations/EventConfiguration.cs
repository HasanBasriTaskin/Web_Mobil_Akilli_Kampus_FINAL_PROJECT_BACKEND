using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired();

            builder.Property(x => x.Location)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Price)
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.ImageUrl)
                .HasMaxLength(500);

            builder.Property(x => x.CreatedByUserId)
                .IsRequired();

            // Optimistic Locking - Version (MySQL uyumlu)
            builder.Property(x => x.Version)
                .IsConcurrencyToken();

            // Index for quick lookup by date
            builder.HasIndex(x => x.StartDate);
            builder.HasIndex(x => x.EndDate);

            // Index for category filter
            builder.HasIndex(x => x.CategoryId);

            // Relationships
            builder.HasOne(x => x.Category)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

