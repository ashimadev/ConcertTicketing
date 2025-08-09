namespace ConcertTicketSystem.Models
{
    public class TicketReservation
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string UserId { get; set; }
        public int TicketTypeId { get; set; }
        public int Quantity { get; set; }
        public DateTime ReservedAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public bool IsPurchased { get; set; }

        public TicketType TicketType { get; set; }
    }
}
