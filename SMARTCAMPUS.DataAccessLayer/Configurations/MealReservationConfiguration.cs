using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class MealReservationConfiguration : IEntityTypeConfiguration<MealReservation>
    {
        public void Configure(EntityTypeBuilder<MealReservation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.QRCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.MealType)
                .HasConversion<string>();

            builder.Property(x => x.Status)
                .HasConversion<string>();

            // Unique QR code
            builder.HasIndex(x => x.QRCode).IsUnique();

            // Unique constraint: One reservation per user per date per meal type
            builder.HasIndex(x => new { x.UserId, x.Date, x.MealType }).IsUnique();

            // Index for quick lookup by status
            builder.HasIndex(x => x.Status);

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Menu)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.MenuId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Cafeteria)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.CafeteriaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
