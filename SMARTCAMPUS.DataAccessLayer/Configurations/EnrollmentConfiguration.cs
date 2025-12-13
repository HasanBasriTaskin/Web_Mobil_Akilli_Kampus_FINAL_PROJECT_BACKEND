using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status).IsRequired().HasMaxLength(20);
            builder.Property(x => x.EnrollmentDate).IsRequired();
            builder.Property(x => x.LetterGrade).HasMaxLength(5);
            builder.Property(x => x.MidtermGrade).HasPrecision(5, 2);
            builder.Property(x => x.FinalGrade).HasPrecision(5, 2);
            builder.Property(x => x.GradePoint).HasPrecision(3, 2);

            builder.HasIndex(x => new { x.StudentId, x.SectionId })
                .IsUnique()
                .HasDatabaseName("IX_Enrollment_Unique");

            builder.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Section)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}



