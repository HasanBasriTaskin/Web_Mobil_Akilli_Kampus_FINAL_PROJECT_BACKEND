using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SMARTCAMPUS.BusinessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Concrete;
using SMARTCAMPUS.DataAccessLayer.Context;
using System.Reflection;
using Xunit;

namespace SMARTCAMPUS.Tests.Managers
{
    public class ScheduleManagerHelperMethodsTests : IDisposable
    {
        private readonly CampusContext _context;
        private readonly ScheduleManager _manager;

        public ScheduleManagerHelperMethodsTests()
        {
            var options = new DbContextOptionsBuilder<CampusContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new CampusContext(options);
            _manager = new ScheduleManager(new UnitOfWork(_context));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void TimesOverlap_ShouldReturnTrue_WhenOverlapping()
        {
            var method = typeof(ScheduleManager).GetMethod("TimesOverlap", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (bool)method!.Invoke(_manager, new object[]
            {
                new TimeSpan(9, 0, 0),
                new TimeSpan(10, 0, 0),
                new TimeSpan(9, 30, 0),
                new TimeSpan(10, 30, 0)
            })!;

            result.Should().BeTrue();
        }

        [Fact]
        public void TimesOverlap_ShouldReturnFalse_WhenNotOverlapping()
        {
            var method = typeof(ScheduleManager).GetMethod("TimesOverlap", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (bool)method!.Invoke(_manager, new object[]
            {
                new TimeSpan(9, 0, 0),
                new TimeSpan(10, 0, 0),
                new TimeSpan(10, 0, 0),
                new TimeSpan(11, 0, 0)
            })!;

            result.Should().BeFalse();
        }

        [Fact]
        public void GetDefaultTimeSlots_ShouldReturnTimeSlots()
        {
            var method = typeof(ScheduleManager).GetMethod("GetDefaultTimeSlots", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = method!.Invoke(_manager, null);

            result.Should().NotBeNull();
            var slots = (List<SMARTCAMPUS.EntityLayer.DTOs.Scheduling.TimeSlotDefinitionDto>)result!;
            slots.Should().HaveCountGreaterThan(0);
            slots.All(s => s.Day != DayOfWeek.Saturday && s.Day != DayOfWeek.Sunday).Should().BeTrue();
        }

        [Fact]
        public void GetSemesterStartDate_ShouldReturnCorrectDate_ForFall()
        {
            var method = typeof(ScheduleManager).GetMethod("GetSemesterStartDate", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (DateTime)method!.Invoke(_manager, new object[] { "Fall", 2024 })!;

            result.Year.Should().Be(2024);
            result.Month.Should().Be(9);
            result.Day.Should().Be(1);
        }

        [Fact]
        public void GetSemesterStartDate_ShouldReturnCorrectDate_ForSpring()
        {
            var method = typeof(ScheduleManager).GetMethod("GetSemesterStartDate", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (DateTime)method!.Invoke(_manager, new object[] { "Spring", 2024 })!;

            result.Year.Should().Be(2024);
            result.Month.Should().Be(2);
            result.Day.Should().Be(1);
        }

        [Fact]
        public void GetFirstOccurrence_ShouldReturnCorrectDate()
        {
            var method = typeof(ScheduleManager).GetMethod("GetFirstOccurrence", BindingFlags.NonPublic | BindingFlags.Instance);
            var start = new DateTime(2024, 9, 1);
            var result = (DateTime)method!.Invoke(_manager, new object[] { start, DayOfWeek.Monday })!;

            result.DayOfWeek.Should().Be(DayOfWeek.Monday);
            result.Should().BeOnOrAfter(start);
        }

        [Fact]
        public void EscapeICalText_ShouldEscapeSpecialCharacters()
        {
            var method = typeof(ScheduleManager).GetMethod("EscapeICalText", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (string)method!.Invoke(_manager, new object[] { "Test,Text;With\\Newline" })!;

            result.Should().Contain("\\,");
            result.Should().Contain("\\;");
            result.Should().Contain("\\\\");
        }

        [Fact]
        public void EscapeICalText_ShouldReturnEmpty_WhenNull()
        {
            var method = typeof(ScheduleManager).GetMethod("EscapeICalText", BindingFlags.NonPublic | BindingFlags.Instance);
            var result = (string)method!.Invoke(_manager, new object[] { (string?)null! })!;

            result.Should().BeEmpty();
        }
    }
}

