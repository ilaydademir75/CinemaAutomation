using CinemaAutomation.Web.Data; // Import application database context
using CinemaAutomation.Web.Data.Entities; // Import entity models
using Microsoft.AspNetCore.Mvc; // Import ASP.NET Core MVC features
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database queries

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers

{
    public class HomeController : Controller // Controller responsible for the public home page
    {
        private readonly CinemaDbContext _context; // Database context field

        // Constructor for dependency injection of the database context
        // _context: EF Core DbContext used to query the database
        public HomeController(CinemaDbContext context)
        {
            _context = context; // Assign injected DbContext to local field
        }

        // GET: /Home/Index
        // Displays active movies that have upcoming showtimes
        public async Task<IActionResult> Index()
        {
            // Query active movies that have at least one future active showtime
            var movies = await _context.Movies
                .Where(m => m.IsActive) // Filter only active movies
                .Where(m => _context.Showtimes.Any(s =>
                    s.MovieId == m.MovieId && // Match showtime movie
                    s.IsActive && // Only active showtimes
                    s.StartTime >= DateTime.Now // Only future showtimes
                ))
                .Select(m => new
                {
                    Movie = m, // Store movie entity
                    NextShowtime = _context.Showtimes
                        .Where(s =>
                            s.MovieId == m.MovieId && // Match showtime movie
                            s.IsActive && // Only active showtimes
                            s.StartTime >= DateTime.Now // Only future showtimes
                        )
                        .Min(s => s.StartTime) // Get nearest upcoming showtime
                })
                .OrderBy(x => x.NextShowtime) // Order movies by their nearest showtime
                .Select(x => x.Movie) // Select only the movie entities
                .ToListAsync(); // Execute query asynchronously

            return View(movies); // Return the home page view with the movie list
        }
    }
}
