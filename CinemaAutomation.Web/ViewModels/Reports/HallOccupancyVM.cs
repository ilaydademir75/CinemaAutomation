namespace CinemaAutomation.Web.ViewModels.Reports
{
    public class HallOccupancyVM
    {
        public int HallId { get; set; }
        public string HallName { get; set; } = null!;

        public int TotalSeats { get; set; }
        public int SoldSeats { get; set; }

        public decimal OccupancyRate { get; set; } // %
    }
}


