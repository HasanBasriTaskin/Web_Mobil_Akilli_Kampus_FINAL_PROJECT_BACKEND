using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class AttendanceSessionConfiguration : IEntityTypeConfiguration<AttendanceSession>
    {
        public void Configure(EntityTypeBuilder<AttendanceSession> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Date).IsRequired();
            builder.Property(x => x.StartTime).IsRequired();
            builder.Property(x => x.EndTime).IsRequired();
            builder.Property(x => x.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Scheduled");
            builder.Property(x => x.QrCode).HasMaxLength(500);
            builder.Property(x => x.Latitude).HasPrecision(10, 8);
            builder.Property(x => x.Longitude).HasPrecision(11, 8);
            builder.Property(x => x.GeofenceRadius).HasPrecision(10, 2);

            builder.HasOne(x => x.Section)
                .WithMany(x => x.AttendanceSessions)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Instructor)
                .WithMany()
                .HasForeignKey(x => x.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}



