using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.Balance)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.Currency)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValue("TRY");

            // Unique constraint: One wallet per user
            builder.HasIndex(x => x.UserId).IsUnique();

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
