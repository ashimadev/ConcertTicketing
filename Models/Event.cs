namespace ConcertTicketSystem.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Venue { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }
        public int Capacity { get; set; }
        public ICollection<TicketType> TicketTypes { get; set; }
    }
}
