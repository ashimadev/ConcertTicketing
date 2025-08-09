using System.Text.Json.Serialization;

namespace ConcertTicketSystem.Models
{
    public class TicketType
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public decimal Price { get; set; }
        public int TotalQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int EventId { get; set; }
    }
}
