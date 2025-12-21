using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class ClassroomReservationConfiguration : IEntityTypeConfiguration<ClassroomReservation>
    {
        public void Configure(EntityTypeBuilder<ClassroomReservation> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.RequestedByUserId)
                .IsRequired();

            builder.Property(x => x.Purpose)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.StudentLeaderName)
                .HasMaxLength(100);

            builder.Property(x => x.Status)
                .HasConversion<string>();

            // Index for conflict checking
            builder.HasIndex(x => new { x.ClassroomId, x.ReservationDate, x.StartTime, x.EndTime });

            // Index for status filter
            builder.HasIndex(x => x.Status);

            // Relationships
            builder.HasOne(x => x.Classroom)
                .WithMany()
                .HasForeignKey(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RequestedBy)
                .WithMany()
                .HasForeignKey(x => x.RequestedByUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
