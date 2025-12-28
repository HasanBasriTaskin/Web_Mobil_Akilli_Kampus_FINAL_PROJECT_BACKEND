using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .HasConversion<string>();

            builder.Property(x => x.ReferenceType)
                .HasConversion<string>();

            builder.Property(x => x.Amount)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.BalanceAfter)
                .HasColumnType("decimal(18,2)");

            builder.Property(x => x.Description)
                .HasMaxLength(200);

            // Index for quick lookup by wallet
            builder.HasIndex(x => x.WalletId);

            // Index for date range queries
            builder.HasIndex(x => x.TransactionDate);

            // Relationships
            builder.HasOne(x => x.Wallet)
                .WithMany(x => x.Transactions)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
