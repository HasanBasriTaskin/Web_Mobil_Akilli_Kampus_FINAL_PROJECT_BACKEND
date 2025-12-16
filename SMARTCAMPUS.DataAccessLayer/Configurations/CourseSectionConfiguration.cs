using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class CourseSectionConfiguration : IEntityTypeConfiguration<CourseSection>
    {
        public void Configure(EntityTypeBuilder<CourseSection> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SectionNumber)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.Semester)
                .IsRequired()
                .HasMaxLength(20);

            // Unique constraint: One section number per course per semester/year
            builder.HasIndex(x => new { x.CourseId, x.SectionNumber, x.Semester, x.Year }).IsUnique();

            // Relationships
            builder.HasOne(x => x.Course)
                .WithMany(x => x.CourseSections)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Instructor)
                .WithMany(x => x.TeachingSections)
                .HasForeignKey(x => x.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(x => x.Enrollments)
                .WithOne(x => x.Section)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.AttendanceSessions)
                .WithOne(x => x.Section)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
