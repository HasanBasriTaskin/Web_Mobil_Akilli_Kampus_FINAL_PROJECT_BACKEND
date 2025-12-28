using FluentAssertions;
using SMARTCAMPUS.EntityLayer.DTOs.Scheduling;
using Xunit;

namespace SMARTCAMPUS.Tests.DTOs.Scheduling
{
    public class WeeklyScheduleDtoTests
    {
        [Fact]
        public void DayName_ShouldReturnDayToString_ForMonday()
        {
            // Arrange
            var dto = new WeeklyScheduleDto
            {
                Day = DayOfWeek.Monday
            };

            // Act
            var dayName = dto.DayName;

            // Assert
            dayName.Should().Be("Monday");
        }

        [Fact]
        public void DayName_ShouldReturnDayToString_ForTuesday()
        {
            // Arrange
            var dto = new WeeklyScheduleDto
            {
                Day = DayOfWeek.Tuesday
            };

            // Act
            var dayName = dto.DayName;

            // Assert
            dayName.Should().Be("Tuesday");
        }

        [Fact]
        public void DayName_ShouldReturnDayToString_ForWednesday()
        {
            // Arrange
            var dto = new WeeklyScheduleDto
            {
                Day = DayOfWeek.Wednesday
            };

            // Act
            var dayName = dto.DayName;

            // Assert
            dayName.Should().Be("Wednesday");
        }

        [Fact]
        public void DayName_ShouldReturnDayToString_ForThursday()
        {
            // Arrange
            var dto = new WeeklyScheduleDto
            {
                Day = DayOfWeek.Thursday
            };

            // Act
            var dayName = dto.DayName;

            // Assert
            dayName.Should().Be("Thursday");
        }

        [Fact]
        public void DayName_ShouldReturnDayToString_ForFriday()
        {
            // Arrange
            var dto = new WeeklyScheduleDto
            {
                Day = DayOfWeek.Friday
            };

            // Act
            var dayName = dto.DayName;

            // Assert
            dayName.Should().Be("Friday");
        }

        [Fact]
        public void DayName_ShouldReturnDayToString_ForSaturday()
        {
            // Arrange
            var dto = new WeeklyScheduleDto
            {
                Day = DayOfWeek.Saturday
            };

            // Act
            var dayName = dto.DayName;

            // Assert
            dayName.Should().Be("Saturday");
        }

        [Fact]
        public void DayName_ShouldReturnDayToString_ForSunday()
        {
            // Arrange
            var dto = new WeeklyScheduleDto
            {
                Day = DayOfWeek.Sunday
            };

            // Act
            var dayName = dto.DayName;

            // Assert
            dayName.Should().Be("Sunday");
        }

        [Fact]
        public void DayName_ShouldReturnCorrectValue_WhenDayChanges()
        {
            // Arrange
            var dto = new WeeklyScheduleDto
            {
                Day = DayOfWeek.Monday
            };

            // Act & Assert - Initial value
            dto.DayName.Should().Be("Monday");

            // Act & Assert - After change
            dto.Day = DayOfWeek.Friday;
            dto.DayName.Should().Be("Friday");
        }

        [Fact]
        public void Schedules_ShouldBeInitialized_AsEmptyList()
        {
            // Arrange & Act
            var dto = new WeeklyScheduleDto();

            // Assert
            dto.Schedules.Should().NotBeNull();
            dto.Schedules.Should().BeEmpty();
        }

        [Fact]
        public void Schedules_ShouldAcceptScheduleList()
        {
            // Arrange
            var dto = new WeeklyScheduleDto();
            var schedules = new List<ScheduleDto>
            {
                new ScheduleDto { Id = 1 },
                new ScheduleDto { Id = 2 }
            };

            // Act
            dto.Schedules = schedules;

            // Assert
            dto.Schedules.Should().HaveCount(2);
            dto.Schedules.Should().BeEquivalentTo(schedules);
        }
    }
}

