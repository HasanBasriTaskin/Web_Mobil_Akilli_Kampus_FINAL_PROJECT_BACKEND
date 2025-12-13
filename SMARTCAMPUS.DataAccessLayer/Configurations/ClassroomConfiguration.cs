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

            builder.Property(x => x.Building).IsRequired().HasMaxLength(50);
            builder.Property(x => x.RoomNumber).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Capacity).IsRequired();
            builder.Property(x => x.FeaturesJson).HasMaxLength(1000);

            builder.HasIndex(x => new { x.Building, x.RoomNumber })
                .IsUnique()
                .HasDatabaseName("IX_Classroom_Unique");
        }
    }
}



