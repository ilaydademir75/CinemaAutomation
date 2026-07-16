using Microsoft.AspNetCore.Authorization; // Import authorization attributes for role-based access control
using Microsoft.AspNetCore.Mvc; // Import MVC base classes and action result types
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for async database queries
using CinemaAutomation.Web.Data; // Import the application database context

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers
{
    [Authorize(Roles = "Company")] // Restrict access to this controller to users with the "Company" role only
    public class CompanyHallsController : Controller
    {
        private readonly CinemaDbContext _context; // Database context instance used to access halls data

        public CompanyHallsController(CinemaDbContext context) // Constructor with dependency injection of CinemaDbContext
        {
            _context = context; // Assign injected DbContext to local field
        }

        // GET: /CompanyHalls/Index
        // Displays the list of halls belonging to the company
        public async Task<IActionResult> Index()
        {
            // CompanyId = 1 represents the logged-in company
            var halls = await _context.Halls
                .Where(h => h.CompanyId == 1 && h.IsActive) // Filter halls by company and active status
                .ToListAsync(); // Execute query asynchronously

            return View(halls); // Return the Index view with the list of halls
        }
    }
}

