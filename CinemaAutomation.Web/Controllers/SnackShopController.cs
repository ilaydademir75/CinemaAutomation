using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult
using Microsoft.EntityFrameworkCore; // Enables EF Core database operations
using CinemaAutomation.Web.Data; // CinemaDbContext
using CinemaAutomation.Web.Data.Entities; // Snack, SnackOrder, SnackOrderItem entities
using CinemaAutomation.Web.ViewModels;  // ViewModels used in snack cart UI
using CinemaAutomation.Web; // SessionExtensions for storing objects in session
using Microsoft.AspNetCore.Authorization; // Authorize / AllowAnonymous attributes
using System.Security.Claims; // Access user claims

namespace CinemaAutomation.Web.Controllers // Define controller namespace
{
    // Require authentication by default for this controller
    [Authorize] // Prevent unauthenticated access to checkout and orders
    public class SnackShopController : Controller
    {
        private readonly CinemaDbContext _context; // Database context field

        public SnackShopController(CinemaDbContext context) // Constructor for dependency injection
        {
            _context = context; // Assign injected DbContext
        }

        // Helper: Get currently logged-in user ID from claims
        private int CurrentUserId
        {
            get
            {
                var val = User?.FindFirst("UserId")?.Value; // Read UserId claim
                return int.TryParse(val, out var id) ? id : 0; // Parse safely or return 0
            }
        }

        // STEP 1: Display active snack products
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Retrieve active snacks from database
            var snacks = await _context.Snacks
                .Where(x => x.IsActive) // Only active products
                .ToListAsync(); // Execute query asynchronously

            return View(snacks); // Return snack list view
        }

        // STEP 2: Add snack to cart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int snackId)
        {
            var snack = await _context.Snacks.FindAsync(snackId); // Retrieve snack by ID

            if (snack == null || !snack.IsActive) // Validate snack existence and active status
            {
                TempData["Error"] = "Product not found."; // Set error message
                return RedirectToAction("Index"); // Redirect back to snack list
            }

            // Stock availability check
            if (snack.StockQuantity <= 0)
            {
                TempData["Error"] = "This product is out of stock."; // Out-of-stock warning
                return RedirectToAction("Index");
            }

            var cart = HttpContext.Session.GetObject<List<int>>("SnackCart") // Retrieve cart from session or create a new one
                       ?? new List<int>(); // Initialize if null

            int alreadyInCart = cart.Count(x => x == snackId); // Check quantity already in cart
            if (alreadyInCart >= snack.StockQuantity) // Prevent adding more than available stock
            {
                TempData["Error"] = "There are no more products in stock.";
                return RedirectToAction("Index");
            }

            cart.Add(snackId); // Add snack ID to cart
            HttpContext.Session.SetObject("SnackCart", cart); // Save updated cart to session

            TempData["Success"] = "The product has been added to the cart."; // Success feedback
            return RedirectToAction("Index"); // Redirect back to snack list
        }

        // STEP 3: Display shopping cart
        [AllowAnonymous]
        public async Task<IActionResult> Cart()
        {
            var cart = HttpContext.Session.GetObject<List<int>>("SnackCart") // Retrieve cart from session
                       ?? new List<int>();

            if (cart.Count == 0) // Handle empty cart
            {
                ViewBag.Total = 0m; // Set total to zero
                return View(new List<SnackCartItemViewModel>()); // Return empty model
            }

            // Group snack IDs and count quantities
            var groups = cart
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            var snackIds = groups.Keys.ToList(); // Extract snack IDs

            // Retrieve snack details from database
            var snacks = await _context.Snacks
                .Where(s => snackIds.Contains(s.SnackId))
                .ToListAsync();

            var model = new List<SnackCartItemViewModel>(); // Initialize cart view model
            decimal total = 0m; // Initialize total price

            foreach (var snack in snacks) // Build cart rows
            {
                int qty = groups[snack.SnackId]; // Quantity per snack
                var row = new SnackCartItemViewModel
                {
                    Snack = snack, // Snack entity
                    Quantity = qty // Quantity selected
                };

                total += row.Subtotal; // Accumulate total price
                model.Add(row); // Add row to model
            }

            ViewBag.Total = total; // Pass total price to view
            return View(model); // Return cart view
        }

        // STEP 4: Checkout and complete order
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            // Redirect unauthenticated users to login
            if (CurrentUserId == 0)
                return RedirectToAction("Login", "Account");

            var cart = HttpContext.Session.GetObject<List<int>>("SnackCart") // Retrieve cart from session
                       ?? new List<int>();

            if (cart.Count == 0) // Prevent checkout with empty cart
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Cart");
            }

            // Group snack IDs and quantities
            var groups = cart
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            var snackIds = groups.Keys.ToList(); // Extract snack IDs

            // Retrieve snack entities
            var snacks = await _context.Snacks
                .Where(s => snackIds.Contains(s.SnackId))
                .ToListAsync();

            // Stock validation before order creation
            foreach (var snack in snacks)
            {
                int requestedQty = groups[snack.SnackId];

                if (snack.StockQuantity < requestedQty)
                {
                    TempData["Error"] =
                        $"Sorry, there is not enough stock for {snack.Name}. Available quantity: {snack.StockQuantity}";
                    return RedirectToAction("Cart");
                }
            }

            decimal total = 0m; // Initialize total amount

            // Create new snack order
            var order = new SnackOrder
            {
                UserId = CurrentUserId, // Assign current user
                OrderDate = DateTime.Now, // Set order date
                TotalAmount = 0m // Temporary total
            };

            _context.SnackOrders.Add(order); // Add order to database
            await _context.SaveChangesAsync(); // Save order to generate primary key

            foreach (var snack in snacks) // Create order items
            {
                int qty = groups[snack.SnackId]; // Quantity ordered
                decimal lineTotal = snack.UnitPrice * qty; // Line total
                total += lineTotal; // Accumulate total

                var item = new SnackOrderItem
                {
                    SnackOrderId = order.SnackOrderId, // Foreign key
                    SnackId = snack.SnackId, // Snack reference
                    Quantity = qty, // Quantity
                    UnitPrice = snack.UnitPrice // Unit price
                };

                _context.SnackOrderItems.Add(item); // Add order item

                snack.StockQuantity -= qty; // Decrease stock
                _context.Snacks.Update(snack); // Update snack stock
            }

            order.TotalAmount = total; // Assign final total

            await _context.Database.ExecuteSqlRawAsync("SET NOCOUNT ON;"); // Prevent extra SQL result sets
            await _context.SaveChangesAsync(); // Save all changes

            HttpContext.Session.Remove("SnackCart"); // Clear cart session

            TempData["Success"] = "Your snack order has been successfully completed."; // Success message
            return RedirectToAction("Index"); // Redirect to snack list
        }

        // STEP 5: Remove item from cart
        [HttpPost]
        [AllowAnonymous]
        public IActionResult RemoveFromCart(int snackId)
        {
            var cart = HttpContext.Session.GetObject<List<int>>("SnackCart") // Retrieve cart from session
                       ?? new List<int>();

            cart.Remove(snackId); // Remove one instance of snack ID
            HttpContext.Session.SetObject("SnackCart", cart); // Save updated cart

            TempData["Success"] = "The product has been removed from the cart."; // Success feedback
            return RedirectToAction("Cart"); // Redirect to cart view
        }

        // Clear entire cart
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("SnackCart"); // Remove cart from session
            TempData["Success"] = "The cart has been cleared."; // Success feedback
            return RedirectToAction("Cart"); // Redirect to cart view
        }

        // Increase item quantity
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Increase(int snackId)
        {
            var cart = HttpContext.Session.GetObject<List<int>>("SnackCart") ?? new List<int>(); // Retrieve cart
            cart.Add(snackId); // Add snack ID
            HttpContext.Session.SetObject("SnackCart", cart); // Save cart
            return RedirectToAction("Cart"); // Redirect to cart
        }


        // Decrease item quantity
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Decrease(int snackId)
        {
            var cart = HttpContext.Session.GetObject<List<int>>("SnackCart") ?? new List<int>(); // Retrieve cart

            if (cart.Contains(snackId)) // Remove one instance if exists
                cart.Remove(snackId);

            HttpContext.Session.SetObject("SnackCart", cart); // Save updated cart

            return RedirectToAction("Cart"); // Redirect to cart
        }


        // Display user's own orders
        public async Task<IActionResult> Orders()
        {
            // Redirect unauthenticated users
            if (CurrentUserId == 0)
                return RedirectToAction("Login", "Account");

            // Retrieve user's snack orders
            var orders = await _context.SnackOrders
                .Where(o => o.UserId == CurrentUserId) // Filter by user
                .OrderByDescending(o => o.OrderDate) // Latest first
                .ToListAsync();

            return View(orders); // Return orders view
        }


        // Display order details
        public async Task<IActionResult> OrderDetails(int id)
        {
            // Redirect unauthenticated users
            if (CurrentUserId == 0)
                return RedirectToAction("Login", "Account");

            // Retrieve order by ID and user
            var order = await _context.SnackOrders
                .FirstOrDefaultAsync(o => o.SnackOrderId == id && o.UserId == CurrentUserId);

            // Return 404 if not found
            if (order == null)
                return NotFound();

            // Retrieve order items with snack names
            var items = await _context.SnackOrderItems
                .Where(i => i.SnackOrderId == id)
                .Join(_context.Snacks,
                      item => item.SnackId,
                      snack => snack.SnackId,
                      (item, snack) => new { snack.Name, item.Quantity, item.UnitPrice })
                .ToListAsync();

            ViewBag.Items = items; // Pass items to view
            return View(order); // Return order details view
        }
    }
}



