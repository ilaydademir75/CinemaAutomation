namespace CinemaAutomation.Web.ViewModels
{
    public class BookingDetailsViewModel
    {
        public int BookingId { get; set; }
        public string MovieName { get; set; } = null!;
        public string HallName { get; set; } = null!;
        public DateTime StartTime { get; set; }

        public List<string> Seats { get; set; } = new();
        public List<string> Tickets { get; set; } = new();

        public decimal TotalAmount { get; set; }
    }
}


