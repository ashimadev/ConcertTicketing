using ConcertTicketSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SalesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets total sales revenue and total tickets sold for an event. Requires authentication.
        /// </summary>
        /// <returns>Total revenue and tickets sold.</returns>
        [Authorize]
        [HttpGet("totals/{eventId}")]
        public async Task<IActionResult> GetTotalSales(int eventId)
        {
            var totalRevenue = await _context.Sales.Where(x=>x.EventId == eventId).SumAsync(s => s.TotalPrice);
            var totalTicketsSold = await _context.Sales.Where(x => x.EventId == eventId)
                .Include(s => s.Reservation)
                .SumAsync(s => s.Reservation.Quantity);

            return Ok(new
            {
                TotalRevenue = totalRevenue,
                TotalTicketsSold = totalTicketsSold
            });
        }
    }
}
