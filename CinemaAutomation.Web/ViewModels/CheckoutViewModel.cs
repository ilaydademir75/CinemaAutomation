namespace CinemaAutomation.Web.ViewModels
{
    public class CheckoutViewModel
    {
        public DateTime StartTime { get; set; }
        public List<CheckoutSeatItem> Items { get; set; } = new();
        public decimal Total { get; set; }
    }

    public class CheckoutSeatItem
    {
        public int SeatId { get; set; }
        public string TicketType { get; set; }
        public decimal Price { get; set; }
    }
}


