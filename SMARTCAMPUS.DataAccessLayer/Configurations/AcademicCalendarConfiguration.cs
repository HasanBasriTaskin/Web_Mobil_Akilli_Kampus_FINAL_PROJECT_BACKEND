using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMARTCAMPUS.EntityLayer.Models;

namespace SMARTCAMPUS.DataAccessLayer.Configurations
{
    public class AcademicCalendarConfiguration : IEntityTypeConfiguration<AcademicCalendar>
    {
        public void Configure(EntityTypeBuilder<AcademicCalendar> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(2000);
            builder.Property(x => x.StartDate).IsRequired();
            builder.Property(x => x.EndDate).IsRequired();
            builder.Property(x => x.EventType).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Semester).HasMaxLength(20);
        }
    }
}

