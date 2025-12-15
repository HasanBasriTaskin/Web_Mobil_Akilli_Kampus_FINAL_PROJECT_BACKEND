using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
    {
        public void Configure(EntityTypeBuilder<Classroom> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Building)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.RoomNumber)
                .IsRequired()
                .HasMaxLength(20);

            // Unique constraint: One room number per building
            builder.HasIndex(x => new { x.Building, x.RoomNumber }).IsUnique();
        }
    }
}
