using ConcertTicketSystem.Controllers;
using ConcertTicketSystem.Data;
using ConcertTicketSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConcertTicketTest.Controllers
{
    public class EventsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly EventsController _controller;

        public EventsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new EventsController(_context);
        }

        [Fact]
        public async Task GetEvents_ReturnsAllEvents()
        {
            // Arrange
            var testEvent = new Event
            {
                Name = "Test Concert",
                Venue = "Test Venue",
                Description = "Test Description",
                EventDate = DateTime.UtcNow.AddDays(30),
                Capacity = 1000,
                TicketTypes = new List<TicketType>
                {
                    new() { Type = "VIP", Price = 200, TotalQuantity = 100, AvailableQuantity = 100 }
                }
            };

            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetEvents();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var events = Assert.IsAssignableFrom<IEnumerable<Event>>(okResult.Value);
            var eventList = events.ToList();
            Assert.Single(eventList);
            Assert.Equal("Test Concert", eventList[0].Name);
            Assert.Single(eventList[0].TicketTypes);
        }

        [Fact]
        public async Task CreateEvent_ValidEvent_ReturnsCreatedEvent()
        {
            // Arrange
            var newEvent = new Event
            {
                Name = "New Concert",
                Venue = "New Venue",
                Description = "New Description",
                EventDate = DateTime.UtcNow.AddDays(30),
                Capacity = 1000,
                TicketTypes = new List<TicketType>
                {
                    new() { Type = "Regular", Price = 100, TotalQuantity = 1000, AvailableQuantity = 1000 }
                }
            };

            // Act
            var result = await _controller.CreateEvent(newEvent);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdEvent = Assert.IsType<Event>(okResult.Value);
            Assert.Equal("New Concert", createdEvent.Name);
            Assert.NotEqual(0, createdEvent.Id);
        }

        [Fact]
        public async Task UpdateEvent_ValidEvent_ReturnsUpdatedEvent()
        {
            // Arrange
            var existingEvent = new Event
            {
                Name = "Original Concert",
                Venue = "Original Venue",
                Description = "Original Description",
                EventDate = DateTime.UtcNow.AddDays(30),
                Capacity = 1000,
                TicketTypes = new List<TicketType>
                {
                    new() { Type = "Regular", Price = 100, TotalQuantity = 1000, AvailableQuantity = 1000 }
                }
            };

            _context.Events.Add(existingEvent);
            await _context.SaveChangesAsync();

            var updatedEvent = new Event
            {
                Name = "Updated Concert",
                Venue = "Updated Venue",
                Description = "Updated Description",
                EventDate = DateTime.UtcNow.AddDays(45),
                Capacity = 1500,
                TicketTypes = new List<TicketType>
                {
                    new() { Type = "VIP", Price = 200, TotalQuantity = 100, AvailableQuantity = 100 },
                    new() { Type = "Regular", Price = 100, TotalQuantity = 1400, AvailableQuantity = 1400 }
                }
            };

            // Act
            var result = await _controller.UpdateEvent(existingEvent.Id, updatedEvent);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedEvent = Assert.IsType<Event>(okResult.Value);
            Assert.Equal("Updated Concert", returnedEvent.Name);
            Assert.Equal(2, returnedEvent.TicketTypes.Count);
            Assert.Contains(returnedEvent.TicketTypes, t => t.Type == "VIP");
            Assert.Contains(returnedEvent.TicketTypes, t => t.Type == "Regular");
        }

        [Fact]
        public async Task UpdateEvent_NonExistentEvent_ReturnsNotFound()
        {
            // Arrange
            var updatedEvent = new Event
            {
                Name = "Updated Concert",
                Venue = "Updated Venue",
                Description = "Updated Description",
                EventDate = DateTime.UtcNow.AddDays(45),
                Capacity = 1500
            };

            // Act
            var result = await _controller.UpdateEvent(999, updatedEvent);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CancelEvent_ValidId_ReturnsOkResult()
        {
            // Arrange
            var testEvent = new Event
            {
                Name = "Test Concert",
                Venue = "Test Venue",
                Description = "Test Description",
                EventDate = DateTime.UtcNow.AddDays(30),
                Capacity = 1000
            };

            _context.Events.Add(testEvent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.CancelEvent(testEvent.Id);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var deletedEvent = await _context.Events.FindAsync(testEvent.Id);
            Assert.Null(deletedEvent);
        }

        [Fact]
        public async Task CancelEvent_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.CancelEvent(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}