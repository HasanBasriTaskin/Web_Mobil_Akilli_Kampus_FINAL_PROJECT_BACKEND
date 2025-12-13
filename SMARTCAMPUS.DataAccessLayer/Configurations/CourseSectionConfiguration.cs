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

            builder.Property(x => x.SectionNumber).IsRequired().HasMaxLength(10);
            builder.Property(x => x.Semester).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Year).IsRequired();
            builder.Property(x => x.Capacity).IsRequired();
            builder.Property(x => x.EnrolledCount).IsRequired().HasDefaultValue(0);
            builder.Property(x => x.ScheduleJson).HasMaxLength(2000);

            builder.HasIndex(x => new { x.CourseId, x.SectionNumber, x.Semester, x.Year })
                .IsUnique()
                .HasDatabaseName("IX_CourseSection_Unique");

            builder.HasOne(x => x.Course)
                .WithMany(x => x.Sections)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Instructor)
                .WithMany()
                .HasForeignKey(x => x.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.Classroom)
                .WithMany(x => x.Sections)
                .HasForeignKey(x => x.ClassroomId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

