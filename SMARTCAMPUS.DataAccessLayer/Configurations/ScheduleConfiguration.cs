using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.DayOfWeek)
                .HasConversion<string>();

            // Unique constraint: No duplicate schedules for same section, day, and time
            builder.HasIndex(x => new { x.SectionId, x.DayOfWeek, x.StartTime }).IsUnique();

            // Index for conflict checking
            builder.HasIndex(x => new { x.ClassroomId, x.DayOfWeek, x.StartTime, x.EndTime });

            // Relationships
            builder.HasOne(x => x.Section)
                .WithMany(x => x.Schedules)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Classroom)
                .WithMany()
                .HasForeignKey(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
