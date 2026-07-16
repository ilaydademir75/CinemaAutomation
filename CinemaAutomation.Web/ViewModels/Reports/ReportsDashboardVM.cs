using System;
using System.Collections.Generic;

namespace CinemaAutomation.Web.ViewModels.Reports
{
    public class ReportsDashboardVM
    {
        public DailySalesVM DailySales { get; set; } = null!;

        public int SoldSeats { get; set; } 

        public List<MovieRevenueVM> MovieRevenues { get; set; } = new();

        public List<HallOccupancyVM> HallOccupancies { get; set; } = new();

        public SnackSalesVM SnackSales { get; set; } = null!;
    }
}


