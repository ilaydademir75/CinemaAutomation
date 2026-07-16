using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult
using Microsoft.EntityFrameworkCore; // Enables async LINQ and EF queries
using CinemaAutomation.Web.Data; // CinemaDbContext definition
using CinemaAutomation.Web.ViewModels.Reports; // ViewModels used for reporting dashboard

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers
{
    public class ReportsController : Controller // Controller responsible for system-wide reports (admin level)
    {
        private readonly CinemaDbContext _context; // Database context field

        public ReportsController(CinemaDbContext context) // Constructor for dependency injection
        {
            _context = context; // Assign injected DbContext
        }

        // GET: /Reports/Index
        // Displays daily reports dashboard
        public async Task<IActionResult> Index(DateTime? date)
        {
            var targetDate = date?.Date ?? DateTime.Today; // Determine target report date (use today if not provided)

            // TICKET REVENUE
            // Calculates total ticket revenue for the selected date
            var ticketRevenue = await _context.Bookings
                .Where(b => // Filter bookings
                    b.PaidAt.HasValue && // Only paid bookings
                    b.PaidAt.Value.Date == targetDate // Match selected date
                )
                .SumAsync(b => b.TotalAmount); // Sum total amounts

            // SNACK REVENUE
            // Calculates total snack revenue for the selected date
            var snackRevenue = await _context.SnackOrders
                .Where(s => // Filter snack orders
                s.OrderDate.Date == targetDate) // Match selected date
                .SumAsync(s => s.TotalAmount); // Sum snack order totals

            // TOTAL SOLD SEATS
            // Counts sold seats for paid bookings on selected date
            var soldSeats = await _context.BookingSeats
                .Join( // Join booking seats with bookings
                    _context.Bookings, // Target bookings table
                    bs => bs.BookingId, // BookingSeat foreign key
                    b => b.BookingId, // Booking primary key
                    (bs, b) => b // Project booking entity
                )
                .Where(b => // Filter bookings
                    b.PaidAt.HasValue && // Only paid bookings
                    b.PaidAt.Value.Date == targetDate // Match selected date
                )
                .CountAsync(); // Count sold seats

            // MOVIE REVENUE
            // Calculates revenue and tickets sold per movie
            var movieRevenues = await _context.Bookings
                .Where(b => // Filter bookings
                    b.PaidAt.HasValue && // Only paid bookings
                    b.PaidAt.Value.Date == targetDate // Match selected date
                )
                .Join( // Join bookings with showtimes
                    _context.Showtimes, // Target showtimes table
                    b => b.ShowtimeId, // Booking foreign key
                    st => st.ShowtimeId, // Showtime primary key
                    (b, st) => new { b.TotalAmount, st.MovieId } // Project anonymous object
                )
                .GroupBy(x => x.MovieId) // Group results by movie
                .Select(g => new MovieRevenueVM // Project into MovieRevenueVM
                {
                    MovieId = g.Key, // Movie identifier
                    TicketsSold = g.Count(), // Number of tickets sold
                    TotalRevenue = g.Sum(x => x.TotalAmount) // Total movie revenue
                })
                .ToListAsync(); // Execute query asynchronously

            // HALL OCCUPANCY
            // Calculates seat occupancy per hall
            var hallOccupancies = await (
                from h in _context.Halls // Iterate through all halls
                select new HallOccupancyVM // Project into HallOccupancyVM
                {
                    HallId = h.HallId, // Hall identifier
                    HallName = h.Name, // Hall name

                    // Total seats in the hall
                    TotalSeats = _context.Seats.Count(s => s.HallId == h.HallId),

                    // Sold seats for the selected date (paid bookings only)
                    SoldSeats = (
                        from bs in _context.BookingSeats
                        join b in _context.Bookings on bs.BookingId equals b.BookingId
                        join st in _context.Showtimes on b.ShowtimeId equals st.ShowtimeId
                        where b.PaidAt.HasValue
                           && b.PaidAt.Value.Date == targetDate
                           && st.HallId == h.HallId
                        select bs
                    ).Count()
                }
            ).ToListAsync(); // Execute occupancy query

            // Calculate occupancy percentage for each hall
            hallOccupancies.ForEach(x =>
            {
                x.OccupancyRate = x.TotalSeats == 0  // Prevent division by zero
                    ? 0
                    : Math.Round((decimal)x.SoldSeats * 100 / x.TotalSeats, 2);
            }); // Calculate occupancy percentage


            // DASHBOARD VIEWMODEL
            // Combine all report sections into a single ViewModel
            var vm = new ReportsDashboardVM
            {
                DailySales = new DailySalesVM // Daily sales summary
                {
                    Date = targetDate, // Report date
                    TicketRevenue = ticketRevenue, // Ticket revenue
                    SnackRevenue = snackRevenue // Snack revenue
                },

                SoldSeats = soldSeats, // Total sold seats
                MovieRevenues = movieRevenues, // Revenue per movie
                HallOccupancies = hallOccupancies, // Occupancy per hall

                SnackSales = new SnackSalesVM // Snack sales summary
                {
                    TotalRevenue = snackRevenue  // Total snack revenue
                }
            };

            return View(vm); // Return dashboard view with populated ViewModel
        }
    }
}










