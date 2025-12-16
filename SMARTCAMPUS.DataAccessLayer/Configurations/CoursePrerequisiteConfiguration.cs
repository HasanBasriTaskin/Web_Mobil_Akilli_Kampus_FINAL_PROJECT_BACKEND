using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class CoursePrerequisiteConfiguration : IEntityTypeConfiguration<CoursePrerequisite>
    {
        public void Configure(EntityTypeBuilder<CoursePrerequisite> builder)
        {
            // Composite primary key
            builder.HasKey(x => new { x.CourseId, x.PrerequisiteCourseId });

            // Relationships - Self-referencing many-to-many
            builder.HasOne(x => x.Course)
                .WithMany(x => x.Prerequisites)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.PrerequisiteCourse)
                .WithMany(x => x.RequiredFor)
                .HasForeignKey(x => x.PrerequisiteCourseId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a course that is a prerequisite
        }
    }
}
