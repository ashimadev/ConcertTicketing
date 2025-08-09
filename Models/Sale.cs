namespace ConcertTicketSystem.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int ReservationId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime PurchasedAt { get; set; }
        public TicketReservation Reservation { get; set; }
    }
}
