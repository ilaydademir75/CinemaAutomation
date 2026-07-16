namespace CinemaAutomation.Web.ViewModels
{
    public class BookingHistoryViewModel
    {
        public int BookingId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public int SeatCount { get; set; }
    }
}

