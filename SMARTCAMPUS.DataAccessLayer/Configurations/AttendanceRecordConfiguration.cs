using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
    {
        public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FlagReason).HasMaxLength(500);
            builder.Property(x => x.Latitude).HasPrecision(10, 8);
            builder.Property(x => x.Longitude).HasPrecision(11, 8);
            builder.Property(x => x.DistanceFromCenter).HasPrecision(10, 2);

            builder.HasIndex(x => new { x.SessionId, x.StudentId })
                .IsUnique()
                .HasDatabaseName("IX_AttendanceRecord_Unique");

            builder.HasOne(x => x.Session)
                .WithMany(x => x.AttendanceRecords)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}



