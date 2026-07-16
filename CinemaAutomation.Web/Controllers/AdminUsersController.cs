using CinemaAutomation.Web.Data; // Import the database context namespace
using Microsoft.AspNetCore.Authorization; // Import authorization attributes for role-based access control
using Microsoft.AspNetCore.Mvc; // Import MVC base classes
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database operations

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers
{
    [Authorize(Roles = "Admin")] // Restrict access to this controller to users with the Admin role only
    public class AdminUsersController : Controller
    {
        private readonly CinemaDbContext _context; // Database context instance for accessing application data

        public AdminUsersController(CinemaDbContext context) // Constructor for dependency injection of CinemaDbContext
        {
            _context = context; // Assign injected context to local field
        }

        // GET: /AdminUsers/Index
        // Displays the list of all users for admin management
        public async Task<IActionResult> Index()
        {
            // Retrieve users from the database including their roles
            var users = await _context.Users
                .Include(u => u.Role) // Load related Role entity
                .OrderBy(u => u.FullName) // Sort users alphabetically by full name
                .ToListAsync(); // Execute query asynchronously and convert to list

            return View(users); // Return the Index view with the user list as model
        }

        // POST: /AdminUsers/ToggleActive
        // Toggles the active/inactive status of a user
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var user = await _context.Users.FindAsync(id); // Find the user by primary key (UserId)
            if (user == null) return NotFound(); // If user is not found, return 404 response

            user.IsActive = !user.IsActive; // Toggle the IsActive flag (active <-> inactive)
            await _context.SaveChangesAsync();  // Save changes to the database

            return RedirectToAction(nameof(Index)); // Redirect back to the user list page
        }
    }
}


