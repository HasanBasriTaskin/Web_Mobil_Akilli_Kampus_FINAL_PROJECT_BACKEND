using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class CoursePrerequisiteConfiguration : IEntityTypeConfiguration<CoursePrerequisite>
    {
        public void Configure(EntityTypeBuilder<CoursePrerequisite> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.CourseId, x.PrerequisiteCourseId })
                .IsUnique()
                .HasDatabaseName("IX_CoursePrerequisite_Unique");

            builder.HasOne(x => x.Course)
                .WithMany(x => x.Prerequisites)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.PrerequisiteCourse)
                .WithMany(x => x.RequiredBy)
                .HasForeignKey(x => x.PrerequisiteCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevent self-referencing prerequisites
            builder.HasCheckConstraint("CK_CoursePrerequisite_NoSelfReference", 
                "CourseId != PrerequisiteCourseId");
        }
    }
}



