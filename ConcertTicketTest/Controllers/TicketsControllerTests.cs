using ConcertTicketSystem.Controllers;
using ConcertTicketSystem.Data;
using ConcertTicketSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace ConcertTicketTest.Controllers
{
    public class TicketsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TicketsController _controller;

        public TicketsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new TicketsController(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser@example.com"),
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetAvailability_ReturnsTicketTypes()
        {
            // Arrange
            var evt = CreateTestEvent();
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAvailability(evt.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tickets = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Single(tickets);
        }

        [Fact]
        public async Task Reserve_ValidRequest_CreatesReservation()
        {
            // Arrange
            var evt = CreateTestEvent();
            await _context.SaveChangesAsync();

            var ticketType = evt.TicketTypes.First();
            var reservationDto = new ReservationDto
            {
                EventId = evt.Id,
                TicketTypeId = ticketType.Id,
                Quantity = 2
            };

            // Act
            var result = await _controller.Reserve(reservationDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var reservation = Assert.IsType<TicketReservation>(okResult.Value);
            Assert.Equal(2, reservation.Quantity);
            Assert.Equal(ticketType.Id, reservation.TicketTypeId);
            Assert.False(reservation.IsPurchased);

            var updatedTicketType = await _context.TicketTypes.FindAsync(ticketType.Id);
            Assert.Equal(98, updatedTicketType.AvailableQuantity);
        }

        [Fact]
        public async Task Reserve_InsufficientTickets_ReturnsBadRequest()
        {
            // Arrange
            var evt = CreateTestEvent();
            await _context.SaveChangesAsync();

            var ticketType = evt.TicketTypes.First();
            var reservationDto = new ReservationDto
            {
                TicketTypeId = ticketType.Id,
                Quantity = 101
            };

            // Act
            var result = await _controller.Reserve(reservationDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CancelReservation_ValidReservation_ReleasesTickets()
        {
            // Arrange
            var evt = CreateTestEvent();
            await _context.SaveChangesAsync();

            var ticketType = evt.TicketTypes.First();
            var reservation = new TicketReservation
            {
                TicketTypeId = ticketType.Id,
                Quantity = 5,
                ReservedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddMinutes(15),
                UserId = "testuser@example.com",
                IsPurchased = false,
                TicketType = ticketType
            };

            ticketType.AvailableQuantity -= reservation.Quantity;
            _context.TicketReservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.CancelReservation(reservation.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedTicketType = await _context.TicketTypes.FindAsync(ticketType.Id);
            Assert.Equal(100, updatedTicketType.AvailableQuantity);
        }

        [Fact]
        public async Task GetReservation_ExistingReservation_ReturnsReservation()
        {
            // Arrange
            var evt = CreateTestEvent();
            await _context.SaveChangesAsync();

            var ticketType = evt.TicketTypes.First();
            var reservation = new TicketReservation
            {
                TicketTypeId = ticketType.Id,
                Quantity = 2,
                ReservedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddMinutes(15),
                UserId = "testuser@example.com",
                IsPurchased = false,
                TicketType = ticketType
            };

            _context.TicketReservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReservation(reservation.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedReservation = Assert.IsType<TicketReservation>(okResult.Value);
            Assert.Equal(reservation.Id, returnedReservation.Id);
            Assert.Equal(2, returnedReservation.Quantity);
        }

        private Event CreateTestEvent()
        {
            var evt = new Event
            {
                Name = "Test Concert",
                Venue = "Test Venue",
                Description = "Test Description",
                EventDate = DateTime.UtcNow.AddDays(30),
                Capacity = 1000,
                TicketTypes = new List<TicketType>
                {
                    new()
                    {
                        Type = "VIP",
                        Price = 200,
                        TotalQuantity = 100,
                        AvailableQuantity = 100
                    }
                }
            };

            _context.Events.Add(evt);
            return evt;
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}