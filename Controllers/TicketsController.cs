using ConcertTicketSystem.Data;
using ConcertTicketSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets ticket availability for a specific event.
        /// </summary>
        /// <param name="eventId">Event ID.</param>
        /// <returns>List of ticket types and their availability.</returns>
        [HttpGet("availability")]
        public async Task<IActionResult> GetAvailability(int eventId)
        {
            var tickets = await _context.TicketTypes
                .Where(t => t.EventId == eventId)
                .ToListAsync();

            return Ok(tickets.Select(t => new {
                t.Type,
                t.Price,
                t.AvailableQuantity
            }));
        }

        /// <summary>
        /// Reserves tickets for a user. Requires authentication.
        /// </summary>
        /// <param name="dto">Reservation details.</param>
        /// <returns>The created reservation or error.</returns>
        [Authorize]
        [HttpPost("reservation")]
        public async Task<IActionResult> Reserve([FromBody] ReservationDto dto)
        {
            await CleanupExpiredReservations();

            var ticketType = await _context.TicketTypes.Where(x=>x.EventId == dto.EventId && x.Id == dto.TicketTypeId).FirstOrDefaultAsync();
            if (ticketType == null || ticketType.AvailableQuantity < dto.Quantity)
                return BadRequest("Insufficient tickets available");

            ticketType.AvailableQuantity -= dto.Quantity;

            var reservation = new TicketReservation
            {
                TicketTypeId = dto.TicketTypeId,
                EventId = dto.EventId,
                Quantity = dto.Quantity,
                ReservedAt = DateTime.UtcNow,
                ExpireAt = DateTime.UtcNow.AddMinutes(15),
                UserId = User.Identity.Name ?? "guest",
                IsPurchased = false
            };

            _context.TicketReservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok(reservation);
        }

        /// <summary>
        /// Reserves tickets for a user. Requires authentication.
        /// </summary>
        /// <param name="dto">Reservation details.</param>
        /// <returns>The created reservation or error.</returns>
        [Authorize]
        [HttpGet("reservation/{id}")]
        public async Task<IActionResult> GetReservation(int id)
        {
            var reservation = await _context.TicketReservations
                .Include(r => r.TicketType)
                .FirstOrDefaultAsync(r => r.Id == id);

            return Ok(reservation);
        }

        /// <summary>
        /// Cancels a ticket reservation by ID if it is not purchased. Requires authentication.
        /// </summary>
        /// <param name="id">Reservation ID.</param>
        /// <returns>Success message or error if not found.</returns>
        [Authorize]
        [HttpDelete("reservations/{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _context.TicketReservations
                .Include(r => r.TicketType)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound("Reservation not found");

            if (reservation.IsPurchased)
                return BadRequest("Cannot cancel a reservation that has been purchased");

            reservation.TicketType.AvailableQuantity += reservation.Quantity;

            _context.TicketReservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return Ok("Reservation cancelled and tickets released");
        }

        /// <summary>
        /// Purchases reserved tickets. Requires authentication.
        /// </summary>
        /// <param name="dto">Purchase details.</param>
        /// <returns>Success message or error if reservation is invalid.</returns>
        [Authorize]
        [HttpPost("purchases")]
        public async Task<IActionResult> Purchase([FromBody] PurchaseDto dto)
        {
            await CleanupExpiredReservations();

            var reservation = await _context.TicketReservations
                .Include(r => r.TicketType)
                .FirstOrDefaultAsync(r => r.Id == dto.ReservationId);

            if (reservation == null || reservation.IsPurchased || reservation.ExpireAt < DateTime.UtcNow)
                return BadRequest("Invalid or expired reservation");

            reservation.IsPurchased = true;

            var sale = new Sale
            {
                EventId = reservation.EventId,
                ReservationId = reservation.Id,
                TotalPrice = reservation.Quantity * reservation.TicketType.Price,
                PurchasedAt = DateTime.UtcNow
            };

            _context.Sales.Add(sale);

            await _context.SaveChangesAsync();

            return Ok("Purchase confirmed");
        }

        /// <summary>
        /// Cleans up expired reservations and releases tickets.
        /// </summary>
        private async Task CleanupExpiredReservations()
        {
            var now = DateTime.UtcNow;

            var expired = await _context.TicketReservations
                .Where(r => !r.IsPurchased && r.ExpireAt < now)
                .ToListAsync();

            foreach (var r in expired)
            {
                var ticketType = await _context.TicketTypes.FindAsync(r.TicketTypeId);
                if (ticketType != null)
                    ticketType.AvailableQuantity += r.Quantity;

                _context.TicketReservations.Remove(r);
            }

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// DTO for ticket reservation.
    /// </summary>
    public class ReservationDto
    {
        /// <summary>
        /// Event ID.
        /// </summary>
        public int EventId { get; set; }
        /// <summary>
        /// Ticket type ID.
        /// </summary>
        public int TicketTypeId { get; set; }
        /// <summary>
        /// Number of tickets to reserve.
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// DTO for ticket purchase.
    /// </summary>
    public class PurchaseDto
    {
        /// <summary>
        /// Reservation ID to purchase.
        /// </summary>
        public int ReservationId { get; set; }
    }
}
