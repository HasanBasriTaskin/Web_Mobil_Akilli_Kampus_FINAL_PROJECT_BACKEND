using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class EventWaitlistConfiguration : IEntityTypeConfiguration<EventWaitlist>
    {
        public void Configure(EntityTypeBuilder<EventWaitlist> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired();

            // Unique constraint: One waitlist entry per user per event
            builder.HasIndex(x => new { x.EventId, x.UserId }).IsUnique();

            // Index for queue position ordering
            builder.HasIndex(x => new { x.EventId, x.QueuePosition });

            // Relationships
            builder.HasOne(x => x.Event)
                .WithMany(x => x.Waitlists)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
