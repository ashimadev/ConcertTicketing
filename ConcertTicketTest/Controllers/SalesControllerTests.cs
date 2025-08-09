using ConcertTicketSystem.Controllers;
using ConcertTicketSystem.Data;
using ConcertTicketSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConcertTicketTest.Controllers
{
    public class SalesControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly SalesController _controller;

        public SalesControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new SalesController(_context);
        }

        [Fact]
        public async Task GetTotalSales_WithSales_ReturnsTotals()
        {
            // Arrange
            var evt = new Event
            {
                Name = "Test Concert",
                Venue = "Test Venue",
                Description = "Test Description",
                EventDate = DateTime.UtcNow.AddDays(30),
                Capacity = 1000
            };
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            var ticketType = new TicketType
            {
                EventId = evt.Id,
                Type = "VIP",
                Price = 200,
                TotalQuantity = 100,
                AvailableQuantity = 90
            };
            _context.TicketTypes.Add(ticketType);
            await _context.SaveChangesAsync();

            var reservation = new TicketReservation
            {
                TicketTypeId = ticketType.Id,
                Quantity = 10,
                ReservedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddMinutes(15),
                UserId = "testuser@example.com",
                IsPurchased = true
            };
            _context.TicketReservations.Add(reservation);
            await _context.SaveChangesAsync();

            var sale = new Sale
            {
                EventId = evt.Id,
                ReservationId = reservation.Id,
                TotalPrice = 2000,
                PurchasedAt = DateTime.UtcNow,
                Reservation = reservation
            };
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetTotalSales(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var revenueProp = value.GetType().GetProperty("TotalRevenue");
            var ticketsProp = value.GetType().GetProperty("TotalTicketsSold");
            Assert.NotNull(revenueProp);
            Assert.NotNull(ticketsProp);
            Assert.Equal(2000m, revenueProp.GetValue(value));
            Assert.Equal(10, ticketsProp.GetValue(value));
        }

        [Fact]
        public async Task GetTotalSales_NoSales_ReturnsZero()
        {
            // Act
            var result = await _controller.GetTotalSales(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var revenueProp = value.GetType().GetProperty("TotalRevenue");
            var ticketsProp = value.GetType().GetProperty("TotalTicketsSold");
            Assert.NotNull(revenueProp);
            Assert.NotNull(ticketsProp);
            Assert.Equal(0m, revenueProp.GetValue(value));
            Assert.Equal(0, ticketsProp.GetValue(value));
        }

        [Fact]
        public async Task GetTotalSales_MultipleSales_ReturnsTotalSum()
        {
            // Arrange
            var evt = new Event
            {
                Name = "Test Concert",
                Venue = "Test Venue",
                Description = "Test Description",
                EventDate = DateTime.UtcNow.AddDays(30),
                Capacity = 1000
            };
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();

            var ticketType = new TicketType
            {
                EventId = evt.Id,
                Type = "Regular",
                Price = 100,
                TotalQuantity = 1000,
                AvailableQuantity = 980
            };
            _context.TicketTypes.Add(ticketType);
            await _context.SaveChangesAsync();

            for (int i = 0; i < 2; i++)
            {
                var reservation = new TicketReservation
                {
                    TicketTypeId = ticketType.Id,
                    Quantity = 10,
                    ReservedAt = DateTime.UtcNow,
                    ExpireAt = DateTime.UtcNow.AddMinutes(15),
                    UserId = $"user{i}@example.com",
                    IsPurchased = true
                };
                _context.TicketReservations.Add(reservation);
                await _context.SaveChangesAsync();

                var sale = new Sale
                {
                    EventId = evt.Id,
                    ReservationId = reservation.Id,
                    TotalPrice = 1000,
                    PurchasedAt = DateTime.UtcNow,
                    Reservation = reservation
                };
                _context.Sales.Add(sale);
            }
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetTotalSales(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;
            var revenueProp = value.GetType().GetProperty("TotalRevenue");
            var ticketsProp = value.GetType().GetProperty("TotalTicketsSold");
            Assert.NotNull(revenueProp);
            Assert.NotNull(ticketsProp);
            Assert.Equal(2000m, revenueProp.GetValue(value));
            Assert.Equal(20, ticketsProp.GetValue(value));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}