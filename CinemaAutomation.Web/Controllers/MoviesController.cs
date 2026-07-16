using System; // Provides basic system types
using System.IO; // Provides basic system types
using System.Linq; // Provides LINQ query capabilities
using System.Threading.Tasks; // Provides async/await support
using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult
using Microsoft.EntityFrameworkCore; // Provides EF Core database functionality
using CinemaAutomation.Web.Data; // Application DbContext
using CinemaAutomation.Web.Data.Entities; // Movie entity definition
using Microsoft.AspNetCore.Http; // Provides IFormFile for file uploads

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers
{
    public class MoviesController : Controller // Controller responsible for movie management
    {
        private readonly CinemaDbContext _context; // Database context field

        public MoviesController(CinemaDbContext context) // Constructor for dependency injection of DbContext
        {
            _context = context; // Assign injected DbContext
        }

        // GET: Movies
        // Displays the list of all movies
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.ToListAsync(); // Retrieve all movies from the database
            return View(movies); // Return the movie list view
        }

        // GET: Movies/Details
        // Displays details of a single movie
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound(); // Check if movie ID is provided

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.MovieId == id); // Retrieve movie by ID
            if (movie == null) return NotFound(); // Return 404 if movie is not found

            return View(movie); // Return movie details view
        }

        // GET: Movies/Create
        // Displays movie creation form
        public IActionResult Create()
        {
            return View(); // Return create movie view
        }

        // POST: Movies/Create
        // Creates a new movie with optional poster upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Movie movie, IFormFile PosterFile)
        {
            // Check if a poster file was uploaded
            if (PosterFile != null && PosterFile.Length > 0)
            {
                // Generate a unique file name using GUID
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(PosterFile.FileName);

                // Build the full file path for saving the poster
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "posters", fileName);

                // Save the uploaded image to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await PosterFile.CopyToAsync(stream); // Copy file contents
                }

                movie.PosterPath = "/posters/" + fileName; // Save relative poster path to the movie entity
            }

            movie.IsActive = true; // Mark the movie as active
            movie.CreatedAt = DateTime.Now; // Set creation timestamp

            if (ModelState.IsValid) // Validate model state
            {
                _context.Add(movie); // Add movie to database context
                await _context.SaveChangesAsync(); // Persist changes to database
                return RedirectToAction(nameof(Index)); // Redirect to movie list page
            }

            return View(movie); // Return create view with validation errors
        }

        // GET: Movies/Edit
        // Displays movie edit form
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound(); // Check if movie ID is provided

            var movie = await _context.Movies.FindAsync(id); // Retrieve movie by ID
            if (movie == null) return NotFound(); // Return 404 if movie is not found

            return View(movie); // Return edit movie view
        }

        // POST: Movies/Edit
        // Updates an existing movie and optional poster
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Movie movie, IFormFile PosterFile)
        {
            if (id != movie.MovieId) return NotFound(); // Validate route ID against model ID

            if (PosterFile != null && PosterFile.Length > 0) // Check if a new poster file was uploaded
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(PosterFile.FileName); // Generate unique file name
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "posters", fileName);  // Build file path

                using (var stream = new FileStream(filePath, FileMode.Create)) // Save uploaded poster to disk
                {
                    await PosterFile.CopyToAsync(stream);
                }

                movie.PosterPath = "/posters/" + fileName; // Update poster path
            }

            try
            {
                _context.Update(movie); // Update movie entity
                await _context.SaveChangesAsync(); // Save changes to database
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(movie.MovieId)) // Check if movie still exists
                {
                    return NotFound();  // Return 404 if deleted
                }
                else
                {
                    throw; // Re-throw exception
                }
            }

            return RedirectToAction(nameof(Index)); // Redirect to movie list page
        }

        // GET: Movies/Delete
        // Displays delete confirmation
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound(); // Check if movie ID is provided

            var movie = await _context.Movies.FirstOrDefaultAsync(m => m.MovieId == id); // Retrieve movie by ID
            if (movie == null) return NotFound(); // Return 404 if movie is not found

            return View(movie); // Return delete confirmation view
        }

        // POST: Movies/Delete
        // Deletes the movie permanently
        [HttpPost, ActionName("Delete")] // Map POST to Delete action
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id); // Retrieve movie by ID
            // Check if movie exists
            if (movie != null)
            {
                _context.Movies.Remove(movie); // Remove movie from database
            }

            await _context.SaveChangesAsync(); // Save changes
            return RedirectToAction(nameof(Index)); // Redirect to movie list page
        }

        // Helper method: checks if movie exists
        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieId == id); // Return true if a movie with given ID exists
        }
    }
}



