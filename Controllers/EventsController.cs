using ConcertTicketSystem.Data;
using ConcertTicketSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets all events with their ticket types.
        /// </summary>
        /// <returns>List of events.</returns>
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var events = await _context.Events.Include(e => e.TicketTypes).ToListAsync();
            return Ok(events);
        }

        /// <summary>
        /// Creates a new event. Requires authentication.
        /// </summary>
        /// <param name="evt">Event details.</param>
        /// <returns>The created event.</returns>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event evt)
        { 
            _context.Events.Add(evt);
            await _context.SaveChangesAsync();
            return Ok(evt);
        }

        /// <summary>
        /// Updates an existing event by ID. Requires authentication.
        /// </summary>
        /// <param name="id">Event ID.</param>
        /// <param name="updated">Updated event details.</param>
        /// <returns>The updated event or error if not found.</returns>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updated)
        {
            var existing = await _context.Events.Include(e => e.TicketTypes).Where(x => x.Id == id).FirstOrDefaultAsync();
            if (existing == null)
            return NotFound("Event not found");

            _context.TicketTypes.RemoveRange(existing.TicketTypes);

            _context.Entry(existing).CurrentValues.SetValues(new
            {
                updated.Name,
                updated.Venue,
                updated.Description,
                updated.EventDate,
                updated.Capacity
            });

            if (updated.TicketTypes != null)
            {
                foreach (var ticketType in updated.TicketTypes)
                {
                    ticketType.EventId = id;
                    _context.TicketTypes.Add(ticketType);
                }
            }

            await _context.SaveChangesAsync();

            var updatedEvent = await _context.Events
                .Include(e => e.TicketTypes)
                .FirstAsync(e => e.Id == id);

            return Ok(updatedEvent);
        }

        /// <summary>
        /// Cancels (deletes) an event by ID. Requires authentication.
        /// </summary>
        /// <param name="id">Event ID.</param>
        /// <returns>Success message or error if not found.</returns>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelEvent(int id)
        {
            var events = await _context.Events.FirstOrDefaultAsync(r => r.Id == id);

            if (events == null)
                return NotFound("Event not found");

            _context.Events.Remove(events);
            await _context.SaveChangesAsync();

            return Ok("Event cancelled has been cancelled");
        }

    }
}
