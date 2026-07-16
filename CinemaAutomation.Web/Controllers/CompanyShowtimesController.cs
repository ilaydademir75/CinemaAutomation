using CinemaAutomation.Web.Data; // Import application database context
using CinemaAutomation.Web.Data.Entities; // Import entity models
using Microsoft.AspNetCore.Authorization; // Import authorization attributes
using Microsoft.AspNetCore.Mvc; // Import MVC core features
using Microsoft.AspNetCore.Mvc.Rendering; // Import SelectList for dropdowns
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers
{
    [Authorize(Roles = "Company")] // Restrict access to users with the "Company" role
    public class CompanyShowtimesController : Controller
    {
        private readonly CinemaDbContext _context; // Database context instance

        public CompanyShowtimesController(CinemaDbContext context) // Constructor for dependency injection
        {
            _context = context; // Assign injected DbContext
        }

        // GET: /CompanyShowtimes
        // Displays all showtimes with occupancy information
        public async Task<IActionResult> Index()
        {
            // Retrieve all showtimes ordered by start time descending
            var showtimes = await _context.Showtimes
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();

            // Create a dictionary of MovieId -> Movie Title for display
            ViewBag.MovieNames = await _context.Movies
                .ToDictionaryAsync(m => m.MovieId, m => m.Title);

            // Create a dictionary of HallId -> Hall Name for display
            ViewBag.HallNames = await _context.Halls
                .ToDictionaryAsync(h => h.HallId, h => h.Name);

            // Dictionary to store occupancy rate per showtime
            var occupancy = new Dictionary<int, int>();

            // Loop through each showtime to calculate occupancy
            foreach (var s in showtimes)
            {
                // Count total seats for this showtime
                var totalSeats = await _context.ShowtimeSeats
                    .CountAsync(ss => ss.ShowtimeId == s.ShowtimeId);

                // Count sold seats for this showtime
                var soldSeats = await _context.ShowtimeSeats
                    .CountAsync(ss =>
                        ss.ShowtimeId == s.ShowtimeId &&
                        ss.Status == "Sold");

                // Calculate occupancy percentage
                int rate = totalSeats == 0
                    ? 0
                    : (int)((double)soldSeats / totalSeats * 100);

                // Store occupancy rate keyed by ShowtimeId
                occupancy[s.ShowtimeId] = rate;
            }

            ViewBag.OccupancyRates = occupancy; // Pass occupancy rates to the view

            return View(showtimes); // Return the view with showtime list
        }

        // GET: /CompanyShowtimes/Create
        // Displays form to create a new showtime
        public IActionResult Create()
        {
            // Populate active movies for dropdown selection
            ViewBag.Movies = new SelectList(
                _context.Movies.Where(m => m.IsActive),
                "MovieId",
                "Title"
            );

            // Populate active halls for dropdown selection
            ViewBag.Halls = new SelectList(
                _context.Halls.Where(h => h.IsActive),
                "HallId",
                "Name"
            );

            return View(); // Return create view
        }

        // POST: /CompanyShowtimes/Create
        // Saves a new showtime to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Showtime showtime)
        {
            // Validate incoming model
            if (!ModelState.IsValid)
            {
                return View(showtime); // Return form with validation errors
            }

            showtime.IsActive = true; // Mark showtime as active

            _context.Showtimes.Add(showtime); // Add showtime to database
            await _context.SaveChangesAsync(); // Save changes

            return RedirectToAction(nameof(Index)); // Redirect to index page
        }

        // GET: /CompanyShowtimes/Edit
        // Displays edit form for a showtime
        public async Task<IActionResult> Edit(int id)
        {
            // Retrieve showtime by ID
            var showtime = await _context.Showtimes.FindAsync(id);
            // Return 404 if not found
            if (showtime == null)
                return NotFound();

            // Populate movie dropdown with selected value
            ViewBag.Movies = new SelectList(
                _context.Movies.Where(m => m.IsActive),
                "MovieId",
                "Title",
                showtime.MovieId
            );

            // Populate hall dropdown with selected value
            ViewBag.Halls = new SelectList(
                _context.Halls.Where(h => h.IsActive),
                "HallId",
                "Name",
                showtime.HallId
            );

            return View(showtime); // Return edit view
        }

        // POST: /CompanyShowtimes/Edit
        // Updates an existing showtime
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int ShowtimeId,
            int MovieId,
            int HallId,
            DateTime StartTime,
            string TicketBasePrice)
        {
            // Retrieve the showtime record
            var showtime = await _context.Showtimes
                .FirstOrDefaultAsync(s => s.ShowtimeId == ShowtimeId);

            // Return 404 if not found
            if (showtime == null)
                return NotFound();

            // Normalize decimal separator (culture-safe input handling)
            TicketBasePrice = TicketBasePrice.Replace(".", ",");
            // Parse ticket base price
            decimal price = decimal.Parse(TicketBasePrice);

            // Update showtime fields
            showtime.MovieId = MovieId;
            showtime.HallId = HallId;
            showtime.StartTime = StartTime;
            showtime.TicketBasePrice = price;

            await _context.SaveChangesAsync(); // Save changes
            return RedirectToAction(nameof(Index)); // Redirect to index page
        }


        // POST: /CompanyShowtimes/Delete
        // Soft deletes a showtime
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Retrieve showtime by ID
            var showtime = await _context.Showtimes.FindAsync(id);
            // Return 404 if not found
            if (showtime == null)
                return NotFound();

            showtime.IsActive = false; // Soft delete by marking inactive
            await _context.SaveChangesAsync(); // Save changes

            return RedirectToAction(nameof(Index)); // Redirect to index page
        }
    }
}



