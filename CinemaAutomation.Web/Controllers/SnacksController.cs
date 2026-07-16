using System; // Provides basic system types
using System.Collections.Generic; // Provides generic collection types
using System.Linq; // Provides LINQ query capabilities
using System.Threading.Tasks; // Provides async/await support
using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult
using Microsoft.AspNetCore.Mvc.Rendering; // Provides SelectList for dropdowns
using Microsoft.EntityFrameworkCore;  // Provides EF Core database functionality
using CinemaAutomation.Web.Data; // CinemaDbContext
using CinemaAutomation.Web.Data.Entities; // Snack entity definition

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers
{
    public class SnacksController : Controller // Controller responsible for snack (buffet) management
    {
        private readonly CinemaDbContext _context; // Database context field

        public SnacksController(CinemaDbContext context) // Constructor for dependency injection of DbContext
        {
            _context = context; // Assign injected DbContext
        }

        // GET: Snacks
        // Displays the list of snacks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Snacks.ToListAsync()); // Retrieve all snacks from the database
        }

        // GET: Snacks/Details
        // Displays details of a single snack
        public async Task<IActionResult> Details(int? id)
        {
            // Check if snack ID is provided
            if (id == null)
            {
                return NotFound(); // Return 404 if ID is missing
            }

            // Retrieve snack by ID
            var snack = await _context.Snacks
                .FirstOrDefaultAsync(m => m.SnackId == id);
            // Check if snack exists
            if (snack == null)
            {
                return NotFound(); // Return 404 if not found
            }

            return View(snack); // Return snack details view
        }

        // GET: Snacks/Create
        // Displays snack creation form
        public IActionResult Create()
        {
            return View();  // Return create snack view
        }

        // POST: Snacks/Create
        // Creates a new snack item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SnackId,Name,UnitPrice,StockQuantity,IsActive")] Snack snack)
        {
            if (ModelState.IsValid) // Check if model validation passed
            {
                _context.Add(snack); // Add snack entity to database context
                await _context.SaveChangesAsync(); // Save changes to database
                return RedirectToAction(nameof(Index)); // Redirect to snack list
            }
            return View(snack); // Return view with validation errors
        }

        // GET: Snacks/Edit
        // Displays snack edit form
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if snack ID is provided
            if (id == null)
            {
                return NotFound(); // Return 404 if missing
            }

            var snack = await _context.Snacks.FindAsync(id); // Retrieve snack by ID
            // Check if snack exists
            if (snack == null)
            {
                return NotFound(); // Return 404 if not found
            }
            return View(snack); // Return edit snack view
        }

        // POST: Snacks/Edit
        // Updates an existing snack
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SnackId,Name,UnitPrice,StockQuantity,IsActive")] Snack snack)
        {
            if (id != snack.SnackId) // Validate route ID against model ID
            {
                return NotFound(); // Return 404 on mismatch
            }

            if (ModelState.IsValid) // Check if model validation passed
            {
                try
                {
                    _context.Update(snack); // Update snack entity
                    await _context.SaveChangesAsync(); // Save changes to database
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Check if snack still exists
                    if (!SnackExists(snack.SnackId))
                    {
                        return NotFound(); // Return 404 if deleted
                    }
                    else
                    {
                        throw; // Re-throw exception
                    }
                }
                return RedirectToAction(nameof(Index)); // Redirect to snack list
            }
            return View(snack); // Return view with validation errors
        }

        // GET: Snacks/Delete
        // Displays delete confirmation
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) // Check if snack ID is provided
            {
                return NotFound(); // Return 404 if missing
            }

            // Retrieve snack by ID
            var snack = await _context.Snacks
                .FirstOrDefaultAsync(m => m.SnackId == id);
            // Check if snack exists
            if (snack == null)
            {
                return NotFound(); // Return 404 if not found
            }

            return View(snack); // Return delete confirmation view
        }

        // POST: Snacks/Delete
        // Deletes the snack item
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var snack = await _context.Snacks.FindAsync(id); // Retrieve snack by ID
            // Check if snack exists
            if (snack != null)
            {
                _context.Snacks.Remove(snack); // Remove snack from database
            }

            await _context.SaveChangesAsync(); // Save changes
            return RedirectToAction(nameof(Index)); // Redirect to snack list
        }

        // Helper method: checks if snack exists
        private bool SnackExists(int id)
        {
            return _context.Snacks.Any(e => e.SnackId == id); // Return true if snack with given ID exists
        }
    }
}
