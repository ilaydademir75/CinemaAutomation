namespace CinemaAutomation.Web.ViewModels.Reports
{
    public class DailySalesVM
    {
        public DateTime Date { get; set; }
        public decimal TicketRevenue { get; set; }
        public decimal SnackRevenue { get; set; }
        public decimal TotalRevenue => TicketRevenue + SnackRevenue;
    }
}


