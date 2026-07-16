// Import base system namespaces
using System; // Provides base system types
using System.Collections.Generic; // Provides generic collection types
using System.Linq; // Provides LINQ query capabilities
using System.Threading.Tasks; // Provides async/await task support
// Import ASP.NET Core MVC features
using Microsoft.AspNetCore.Mvc; // Provides MVC controller and action support
// Import Entity Framework Core
using Microsoft.EntityFrameworkCore; // Provides EF Core database features
// Import application database context
using CinemaAutomation.Web.Data; // Application DbContext
// Import entity models
using CinemaAutomation.Web.Data.Entities; // Entity classes

namespace CinemaAutomation.Web.Controllers // Define controller namespace
{
    public class HallsController : Controller // Controller responsible for managing cinema halls
    {
        private readonly CinemaDbContext _context; // Database context field

        public HallsController(CinemaDbContext context) // Constructor for dependency injection
        {
            _context = context; // Assign injected DbContext
        }

        // GET: Halls
        // Displays all halls
        public async Task<IActionResult> Index()
        {
            return View(await _context.Halls.ToListAsync()); // Retrieve all halls from database and return to view
        }

        // GET: Halls/Details
        // Displays details of a specific hall
        public async Task<IActionResult> Details(int? id)
        {
            // Check if ID is provided
            if (id == null)
                return NotFound(); // Return 404 if ID is missing

            // Retrieve hall by ID
            var hall = await _context.Halls
                .FirstOrDefaultAsync(m => m.HallId == id);

            // Check if hall exists
            if (hall == null)
                return NotFound(); // Return 404 if not found

            return View(hall); // Return hall details view
        }

        // GET: Halls/Create
        // Displays hall creation form
        public IActionResult Create()
        {
            return View(); // Return empty create view
        }

        // POST: Halls/Create
        // Creates a new hall and its seats
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("HallId,CompanyId,Name,TotalRows,TotalColumns,IsActive")] Hall hall)
        {
            // Check model validation
            if (ModelState.IsValid)
            {
                // Assign default company ID (single-company system)
                hall.CompanyId = 1;

                // Add hall entity to database
                _context.Add(hall);
                await _context.SaveChangesAsync(); // Save hall first to generate HallId

                // Automatically generate seats for the hall
                for (int row = 1; row <= hall.TotalRows; row++)
                {
                    // Loop through each column
                    for (int col = 1; col <= hall.TotalColumns; col++)
                    {
                        // Create a new seat entity
                        Seat seat = new Seat
                        {
                            HallId = hall.HallId, // Associate seat with hall
                            RowNumber = row, // Set row number
                            SeatNumber = col, // Set seat number
                            IsActive = true // Activate seat
                        };

                        _context.Seats.Add(seat); // Add seat to database context
                    }
                }

                await _context.SaveChangesAsync(); // Save all generated seats

                return RedirectToAction(nameof(Index)); // Redirect to hall list
            }

            return View(hall);  // Return view with validation errors
        }

        // GET: Halls/Edit
        // Displays edit form for a hall
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if ID is provided
            if (id == null)
                return NotFound(); // Return 404 if missing

            // Retrieve hall by ID
            var hall = await _context.Halls.FindAsync(id);

            // Check if hall exists
            if (hall == null)
                return NotFound(); // Return 404 if not found

            return View(hall); // Return edit view
        }

        // POST: Halls/Edit
        // Updates hall information
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("HallId,CompanyId,Name,TotalRows,TotalColumns,IsActive")] Hall hall)
        {
            // Validate route ID against model ID
            if (id != hall.HallId)
                return NotFound();  // Return 404 on mismatch

            // Check model validation
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hall); // Update hall entity
                    await _context.SaveChangesAsync(); // Save changes
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Check if hall still exists
                    if (!HallExists(hall.HallId))
                        return NotFound(); // Return 404 if deleted
                    else
                        throw;  // Re-throw exception
                }
                return RedirectToAction(nameof(Index)); // Redirect to hall list
            }

            return View(hall); // Return view with validation errors
        }

        // GET: Halls/Delete
        // Displays delete confirmation
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if ID is provided
            if (id == null)
                return NotFound(); // Return 404 if missing

            // Retrieve hall by ID
            var hall = await _context.Halls
                .FirstOrDefaultAsync(m => m.HallId == id);

            // Check if hall exists
            if (hall == null)
                return NotFound(); // Return 404 if not found

            return View(hall); // Return delete confirmation view
        }

        // POST: Halls/Delete
        // Soft deletes hall and related showtimes
        [HttpPost, ActionName("Delete")] // Map to Delete action name
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Retrieve hall by ID
            var hall = await _context.Halls.FindAsync(id);

            // Check if hall exists
            if (hall == null)
                return NotFound(); // Return 404 if not found

            // Soft delete hall
            hall.IsActive = false;

            // Retrieve showtimes linked to this hall
            var showtimes = await _context.Showtimes
                .Where(s => s.HallId == id)
                .ToListAsync();

            // Soft delete all related showtimes
            foreach (var s in showtimes)
                s.IsActive = false;

            await _context.SaveChangesAsync(); // Save changes
            return RedirectToAction(nameof(Index)); // Redirect to hall list
        }

        // GET: Halls/ViewSeats
        // Displays seat layout for a hall
        public async Task<IActionResult> ViewSeats(int id)
        {
            // Retrieve seats for given hall
            var seats = await _context.Seats
                .Where(s => s.HallId == id)
                .OrderBy(s => s.RowNumber) // Order by row
                .ThenBy(s => s.SeatNumber) // Then by seat number
                .ToListAsync();

            return View(seats); // Return seat list view
        }

        // Helper method: check if hall exists
        private bool HallExists(int id)
        {
            return _context.Halls.Any(e => e.HallId == id);
        }
    }
}

