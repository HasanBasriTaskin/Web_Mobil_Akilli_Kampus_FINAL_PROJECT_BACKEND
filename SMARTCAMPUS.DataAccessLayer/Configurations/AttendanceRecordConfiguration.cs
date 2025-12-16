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

            builder.Property(x => x.FlagReason)
                .HasMaxLength(500);

            // Unique constraint: One check-in per student per session
            builder.HasIndex(x => new { x.SessionId, x.StudentId }).IsUnique();

            // Relationships
            builder.HasOne(x => x.Session)
                .WithMany(x => x.AttendanceRecords)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Student)
                .WithMany(x => x.AttendanceRecords)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
