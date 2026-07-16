using System; // Provides basic system types
using System.Collections.Generic; // Provides generic collection types
using System.Linq; // Provides LINQ query capabilities
using System.Threading.Tasks; // Provides async/await support
using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult
using Microsoft.AspNetCore.Mvc.Rendering; // Provides SelectList for dropdowns
using Microsoft.EntityFrameworkCore; // Provides EF Core database functionality
using CinemaAutomation.Web.Data; // Application DbContext
using CinemaAutomation.Web.Data.Entities; // Entity classes

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers
{
    public class ShowtimesController : Controller // Controller responsible for managing showtimes
    {
        private readonly CinemaDbContext _context; // Database context field

        public ShowtimesController(CinemaDbContext context)
        {
            _context = context; // Assign injected DbContext
        }

        // GET: Showtimes
        // Displays all showtimes
        public async Task<IActionResult> Index()
        {
            // Retrieve all showtimes including related movie and hall data
            var showtimes = await _context.Showtimes
                .Include(s => s.Movie) // Include movie navigation property
                .Include(s => s.Hall) // Include hall navigation property
                .ToListAsync(); // Execute query asynchronously

            return View(showtimes); // Return the showtimes list view
        }

        // GET: Showtimes/Details
        // Displays details of a specific showtime
        public async Task<IActionResult> Details(int? id)
        {
            // Check if showtime ID is provided
            if (id == null)
            {
                return NotFound(); // Return 404 if ID is missing
            }

            // Retrieve showtime by ID
            var showtime = await _context.Showtimes
                .FirstOrDefaultAsync(m => m.ShowtimeId == id);
            // Check if showtime exists
            if (showtime == null)
            {
                return NotFound(); // Return 404 if not found
            }

            return View(showtime); // Return details view
        }

        // GET: Showtimes/Create
        // Displays create showtime form
        public IActionResult Create()
        {
            ViewBag.Movies = new SelectList(_context.Movies.Where(x => x.IsActive), "MovieId", "Title"); // Populate active movies for dropdown selection
            ViewBag.Halls = new SelectList(_context.Halls.Where(x => x.IsActive), "HallId", "Name"); // Populate active halls for dropdown selection

            return View(); // Return create view
        }


        // POST: Showtimes/Create
        // Creates a new showtime and generates seats
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ShowtimeId,MovieId,HallId,StartTime,TicketBasePrice,IsActive")]
            Showtime showtime) // Bind allowed properties only
        {
            // Remove TicketBasePrice from ModelState to handle culture-specific parsing
            ModelState.Remove(nameof(showtime.TicketBasePrice));

            var priceText = Request.Form["TicketBasePrice"].ToString(); // Read ticket price value from the request
            priceText = priceText.Replace(".", ","); // Normalize decimal separator (dot to comma)

            if (decimal.TryParse(priceText, out var price)) // Try to parse ticket price safely
            {
                showtime.TicketBasePrice = price; // Assign parsed price
            }
            else
            {
                ModelState.AddModelError("TicketBasePrice", "Invalid price format"); // Add model error if parsing fails
            }

            // Check model validation state
            if (ModelState.IsValid)
            {
                _context.Add(showtime);  // Add showtime to database
                await _context.SaveChangesAsync(); // Save showtime to generate ShowtimeId

                var hall = await _context.Halls.FindAsync(showtime.HallId); // Retrieve hall information

                // Check if hall exists
                if (hall != null)
                {
                    // Loop through hall rows
                    for (int row = 1; row <= hall.TotalRows; row++)
                    {
                        // Loop through hall columns
                        for (int seat = 1; seat <= hall.TotalColumns; seat++)
                        {
                            _context.ShowtimeSeats.Add(new ShowtimeSeat // Create ShowtimeSeat record for each physical seat
                            {
                                ShowtimeId = showtime.ShowtimeId, // Assign showtime
                                SeatId = _context.Seats
                                    .Where(s => s.HallId == hall.HallId && // Match hall
                                                s.RowNumber == row && // Match row
                                                s.SeatNumber == seat) // Match seat
                                    .Select(s => s.SeatId) // Select seat ID
                                    .First(), // Get first match
                                Status = "Available" // Initial seat status
                            });
                        }
                    }

                    await _context.SaveChangesAsync(); // Save generated showtime seats
                }

                return RedirectToAction(nameof(Index)); // Redirect to showtime list
            }

            // Repopulate dropdowns if validation fails
            ViewBag.Movies = new SelectList(_context.Movies.Where(x => x.IsActive), "MovieId", "Title");
            ViewBag.Halls = new SelectList(_context.Halls.Where(x => x.IsActive), "HallId", "Name");

            return View(showtime); // Return create view with validation errors
        }

        // GET: Showtimes/Edit
        // Displays edit form for a showtime
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if ID is provided
            if (id == null)
            {
                return NotFound(); // Return 404 if missing
            }

            var showtime = await _context.Showtimes.FindAsync(id); // Retrieve showtime by ID
            if (showtime == null) // Check if showtime exists
            {
                return NotFound(); // Return 404 if not found
            }
            return View(showtime); // Return edit view
        }

        // POST: Showtimes/Edit
        // Updates showtime information
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, // Showtime identifier
            DateTime StartTime, // Updated start time
            decimal TicketBasePrice, // Updated ticket price
            bool IsActive) // Updated active status
        {
            // Retrieve showtime record
            var showtime = await _context.Showtimes
                .FirstOrDefaultAsync(s => s.ShowtimeId == id);

            // Check if showtime exists
            if (showtime == null)
                return NotFound(); // Return 404 if not found

            // Update showtime fields
            showtime.StartTime = StartTime; // Update start time
            showtime.TicketBasePrice = TicketBasePrice; // Update price
            showtime.IsActive = IsActive; // Update active flag

            await _context.SaveChangesAsync(); // Save changes
            return RedirectToAction(nameof(Index)); // Redirect to showtime list
        }


        // GET: Showtimes/Delete
        // Displays delete confirmation
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if ID is provided
            if (id == null)
            {
                return NotFound(); // Return 404 if missing
            }

            // Retrieve showtime by ID
            var showtime = await _context.Showtimes
                .FirstOrDefaultAsync(m => m.ShowtimeId == id);
            // Check if showtime exists
            if (showtime == null)
            {
                return NotFound(); // Return 404 if not found
            }

            return View(showtime); // Return delete confirmation view
        }

        // POST: Showtimes/Delete
        // Soft deletes a showtime
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var showtime = await _context.Showtimes.FindAsync(id); // Retrieve showtime by ID

            // Check if showtime exists
            if (showtime == null)
                return NotFound(); // Return 404 if not found

            showtime.IsActive = false; // Soft delete showtime

            await _context.SaveChangesAsync(); // Save changes
            return RedirectToAction(nameof(Index)); // Redirect to showtime list
        }

        // Helper method: check if showtime exists
        private bool ShowtimeExists(int id)
        {
            return _context.Showtimes.Any(e => e.ShowtimeId == id); // Return true if showtime exists
        }

        // GET: Showtimes/ListByMovie
        // Lists future showtimes for a specific movie
        public async Task<IActionResult> ListByMovie(int movieId)
        {
            var movie = await _context.Movies.FindAsync(movieId); // Retrieve movie by ID

            // Check if movie exists
            if (movie == null)
                return NotFound(); // Return 404 if not found

            // Retrieve active future showtimes for the movie
            var showtimes = await _context.Showtimes
               .Include(s => s.Hall) // Include hall information
               .Where(s =>
                   s.MovieId == movieId && // Match movie
                   s.IsActive && // Only active showtimes
                   s.StartTime >= DateTime.Now // Hide past showtimes
               )
               .OrderBy(s => s.StartTime) // Order by start time
               .ToListAsync(); // Execute query


            ViewBag.MovieTitle = movie.Title; // Pass movie title to the view

            return View(showtimes); // Return showtimes list view
        }
    }
}
