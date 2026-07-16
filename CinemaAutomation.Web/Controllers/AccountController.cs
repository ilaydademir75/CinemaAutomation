using CinemaAutomation.Web.Data; // Import application database context namespace
using CinemaAutomation.Web.Data.Entities; // Import entity models
// Import authentication-related namespaces
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc; // Import MVC base classes
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database operations
using System.Security.Claims; // Import claims-based identity support

namespace CinemaAutomation.Web.Controllers // Define controller namespace
{
    // Controller responsible for authentication and user account operations
    public class AccountController : Controller 
    {
        private readonly CinemaDbContext _context; // Database context instance

        public AccountController(CinemaDbContext context) // Constructor for dependency injection of DbContext
        {
            _context = context; // Assign injected context
        }

        // GET: /Account/Login
        // Displays login page
        public IActionResult Login()
        {
            return View(); // Return Login view
        }

        // POST: /Account/Login
        // Handles login form submission
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Query database for matching active user
            var user = await _context.Users
                .Include(u => u.Role) // Load related Role entity
                .FirstOrDefaultAsync(x =>
                    x.Email == email && // Match email
                    x.PasswordHash == password && // Match password (plain text here)
                    x.IsActive); // Ensure user is active

            // If no user is found, show error
            if (user == null)
            {
                ViewBag.Error = "Invalid email or password."; // Error message
                return View(); // Return Login view
            }

            // Create claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName), // User full name
                new Claim(ClaimTypes.Email, user.Email), // User email
                new Claim(ClaimTypes.Role, user.Role.Name), // User role
                new Claim("UserId", user.UserId.ToString()) // Custom UserId claim
            };

            // Create identity using cookie authentication scheme
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign in user and issue authentication cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            // Redirect user based on role
            if (user.Role.Name == "Admin")
                return RedirectToAction("Index", "Admin"); // Admin dashboard

            if (user.Role.Name == "Company")
                return RedirectToAction("Index", "Company"); // Company panel

            // Default redirect for normal users
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Logout
        // Logs out the current user
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); // Remove authentication cookie
            return RedirectToAction("Login"); // Redirect to Login page
        }

        // GET: /Account/Register
        // Displays registration page
        public IActionResult Register()
        {
            return View(); // Return Register view
        }

        // POST: /Account/Register
        // Handles registration form submission
        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string email, string password)
        {
            // Check if email already exists in database
            var exists = await _context.Users.AnyAsync(x => x.Email == email);

            // If email exists, show error
            if (exists)
            {
                ViewBag.Error = "This email is already registered."; // Error message
                return View(); // Return Register view
            }

            // Retrieve default User role
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Name == "User");

            // If role does not exist, show error
            if (role == null)
            {
                ViewBag.Error = "User role not found. Add it in database.";
                return View();
            }

            // Create new user entity
            var newUser = new User
            {
                FullName = fullName, // Assign full name
                Email = email, // Assign email
                PasswordHash = password, // Store password (not hashed here)
                RoleId = role.RoleId, // Assign User role
                IsActive = true, // Activate account
                CreatedAt = DateTime.Now // Set creation date
            };

            _context.Users.Add(newUser); // Add user to database context
            await _context.SaveChangesAsync(); // Save changes to database

            // Do NOT auto-login after registration
            TempData["RegisterSuccess"] = "Registration successful. Please login."; // Store success message for next request
            return RedirectToAction("Login"); // Redirect user to Login page
        }
    }
}

