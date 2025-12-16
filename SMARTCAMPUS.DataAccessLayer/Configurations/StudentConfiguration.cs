using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            // Primary Key
            builder.HasKey(x => x.Id);

            // Properties
            builder.Property(x => x.StudentNumber)
                .IsRequired()
                .HasMaxLength(20);

            // Relationships
            builder.HasOne(x => x.User)
                .WithOne() // Assuming 1-to-1: One User is one Student (if role matches)
                .HasForeignKey<Student>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Department)
                .WithMany(x => x.Students)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); // Don't delete department if students exist
        }
    }
}
