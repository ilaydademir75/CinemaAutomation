using System.Net.Sockets;
using CinemaAutomation.Web.Data; // Import the application database context
using Microsoft.AspNetCore.Authorization; // Import authorization attributes for role-based access control
using Microsoft.AspNetCore.Mvc; // Import MVC base classes and action result types
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database operations

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers
{
    // Restrict access to this controller to users with the "Company" role only
    [Authorize(Roles = "Company")]
    public class CompanyReportsController : Controller
    {
        private readonly CinemaDbContext _context; // Database context instance used to access reporting data

        // Constructor for dependency injection of CinemaDbContext
        public CompanyReportsController(CinemaDbContext context)
        {
            _context = context; // Assign injected DbContext to local field
        }

        // GET: /CompanyReports/Index
        // Displays daily revenue and occupancy reports for the company
        public async Task<IActionResult> Index(DateTime? date)
        {
            // Determine the target date; use today if no date is provided
            var targetDate = date?.Date ?? DateTime.Today;

            // Ticket Revenue
            // Calculate total ticket revenue for the selected date
            var ticketRevenue = await _context.Bookings
                .Where(b => b.PaidAt.HasValue && // Only paid bookings
                            b.PaidAt.Value.Date == targetDate) // Match selected date
                .SumAsync(b => b.TotalAmount); // Sum total booking amounts

            // Snack Revenue
            // Calculate total snack sales revenue for the selected date
            var snackRevenue = await _context.SnackOrders
                .Where(s => s.OrderDate.Date == targetDate) // Match selected date
                .SumAsync(s => s.TotalAmount); // Sum snack order totals

            // Sold Seats
            // Count total number of sold seats (all time)
            var soldSeats = await _context.BookingSeats
                .CountAsync(); // Count all booking seat records

            // Showtime Occupancy
            // Calculate seat occupancy per showtime
            var occupancies = await _context.ShowtimeSeats
                .GroupBy(x => x.ShowtimeId) // Group seats by showtime
                .Select(g => new
                {
                    ShowtimeId = g.Key, // Showtime identifier
                    Sold = g.Count(x => x.Status == "Sold"), // Number of sold seats
                    Total = g.Count() // Total seats for showtime
                })
                .ToListAsync(); // Execute query and return results

            ViewBag.Date = targetDate; // Pass selected date to the view
            ViewBag.TicketRevenue = ticketRevenue; // Pass calculated ticket revenue to the view
            ViewBag.SnackRevenue = snackRevenue; // Pass total combined revenue to the view
            ViewBag.TotalRevenue = ticketRevenue + snackRevenue; // Pass total sold seat count to the view
            ViewBag.SoldSeats = soldSeats; // Pass total sold seat count to the view
            ViewBag.Occupancies = occupancies;  // Pass showtime occupancy data to the view

            return View(); // Return the Index view with report data
        }
    }
}



