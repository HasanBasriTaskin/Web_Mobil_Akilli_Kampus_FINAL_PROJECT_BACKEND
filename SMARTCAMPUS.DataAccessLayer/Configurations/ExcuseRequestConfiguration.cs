using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class ExcuseRequestConfiguration : IEntityTypeConfiguration<ExcuseRequest>
    {
        public void Configure(EntityTypeBuilder<ExcuseRequest> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Reason)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.DocumentUrl)
                .HasMaxLength(500);

            builder.Property(x => x.Notes)
                .HasMaxLength(1000);

            // Relationships
            builder.HasOne(x => x.Student)
                .WithMany(x => x.ExcuseRequests)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Session)
                .WithMany(x => x.ExcuseRequests)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.ReviewedBy)
                .WithMany()
                .HasForeignKey(x => x.ReviewedById)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
