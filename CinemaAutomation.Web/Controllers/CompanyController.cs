using Microsoft.AspNetCore.Authorization; // Import authorization attributes for role-based access control
using Microsoft.AspNetCore.Mvc; // Import MVC base classes and interfaces

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers in the Cinema Automation project
{
    [Authorize(Roles = "Company")] // Restrict access to this controller to users with the "Company" role only

    public class CompanyController : Controller
    {
        // GET: /Company/Index
        // Displays the main dashboard page for company users
        public IActionResult Index()
        {
            return View(); // Return the default Index view for the Company dashboard
        }

    }
}



