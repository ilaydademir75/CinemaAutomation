using CinemaAutomation.Web.Data; // Import the database context namespace
using CinemaAutomation.Web.ViewModels; // Import ViewModel definitions used in the application
using Microsoft.AspNetCore.Authorization; // Import authorization attributes for role-based access control
using Microsoft.AspNetCore.Mvc; // Import MVC base classes and interfaces
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core (used for database operations if needed)

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers
{
    [Authorize(Roles = "Admin")] // Restrict access to this controller to users with the Admin role only
    public class AdminController : Controller
    {
        // GET: /Admin/Index
        // Displays the admin dashboard main page
        public IActionResult Index()
        {
            return View(); // Return the default Index view for the Admin dashboard
        }
    }
}


