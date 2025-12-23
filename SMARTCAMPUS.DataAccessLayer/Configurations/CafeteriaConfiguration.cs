using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class CafeteriaConfiguration : IEntityTypeConfiguration<Cafeteria>
    {
        public void Configure(EntityTypeBuilder<Cafeteria> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Location)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Capacity)
                .IsRequired();

            // Index for quick lookup
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
