using System; // Import base system functionalities
using System.Collections.Generic; // Import generic collection types
using System.Globalization; // Import culture-specific formatting utilities
using System.Linq; // Import LINQ query extensions
using System.Threading.Tasks; // Import async and task-based programming support
using CinemaAutomation.Web.Data; // Import application database context
using CinemaAutomation.Web.Data.Entities; // Import entity models
using CinemaAutomation.Web.ViewModels; // Import ViewModel classes used for UI data transfer
using Microsoft.AspNetCore.Mvc; // Import MVC framework base classes
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database access
using Microsoft.AspNetCore.Authorization; // Import authorization attributes

namespace CinemaAutomation.Web.Controllers // Define the namespace for controllers
{
    // Require authenticated users for this controller by default
    [Authorize]
    public class BookingController : Controller
    {
        private readonly CinemaDbContext _context; // Database context instance

        public BookingController(CinemaDbContext context) // Constructor for dependency injection of DbContext
        {
            _context = context; // Assign injected context
        }

        // Property to retrieve the currently logged-in user's ID from claims
        private int CurrentUserId
        {
            get
            {
                var val = User?.FindFirst("UserId")?.Value; // Read UserId claim value
                return int.TryParse(val, out var id) ? id : 0; // Try to parse claim value to integer, return 0 if invalid
            }
        }

        // STEP 1: SEAT SELECTION
        public async Task<IActionResult> SelectSeats(int showtimeId)
        {
            // Retrieve showtime information by ID
            var showtime = await _context.Showtimes.FindAsync(showtimeId);

            if (showtime == null) // If showtime does not exist, return 404
                return NotFound();

            // Retrieve all seats for the hall related to this showtime
            var seats = await _context.Seats
                .Where(x => x.HallId == showtime.HallId) // Filter by hall
                .Include(x => x.ShowtimeSeats) // Load seat status per showtime
                .OrderBy(x => x.RowNumber) // Order by row
                .ThenBy(x => x.SeatNumber) // Then order by seat number
                .ToListAsync(); // Execute query

            ViewBag.Showtime = showtime; // Pass showtime info to the view
            return View(seats); // Return seat selection view
        }

        // STEP 2: CONFIRM SEATS
        // Handle seat confirmation form submission
        [HttpPost]
        public IActionResult Confirm(int showtimeId, List<int> SeatIds, Dictionary<int, string> TicketTypes)
        {
            if (SeatIds == null || SeatIds.Count == 0) // Validate that at least one seat is selected
            {
                TempData["Error"] = "You need to choose at least one seat."; // Store error message for next request
                return RedirectToAction("SelectSeats", new { showtimeId }); // Redirect back to seat selection
            }

            TempData["ShowtimeId"] = showtimeId; // Store selected showtime ID temporarily
            TempData["SeatIds"] = string.Join(",", SeatIds); // Store selected seat IDs as comma-separated string
            TempData["TicketTypes"] = string.Join(";", TicketTypes.Select(x => $"{x.Key}:{x.Value}")); // Store ticket types as key:value pairs separated by semicolons

            TempData.Keep(); // Preserve TempData values for the next request
            return RedirectToAction("Checkout"); // Redirect to checkout step
        }

        // STEP 3: CHECKOUT
        // Display checkout summary page
        public async Task<IActionResult> Checkout()
        {
            if (!TempData.ContainsKey("ShowtimeId")) // If required TempData is missing, redirect to seat selection
                return RedirectToAction("SelectSeats");

            int showtimeId = int.Parse(TempData["ShowtimeId"].ToString()); // Parse showtime ID from TempData
            var showtime = await _context.Showtimes.FindAsync(showtimeId); // Retrieve showtime information

            // Parse selected seat IDs from TempData
            List<int> seatIds = TempData["SeatIds"]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            // Parse ticket types dictionary from TempData
            var ticketTypes = TempData["TicketTypes"]
                .ToString()
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(':'))
                .ToDictionary(x => int.Parse(x[0]), x => x[1]);

            decimal total = 0m; // Initialize total price

            var model = new CheckoutViewModel  // Initialize checkout view model
            {
                StartTime = showtime.StartTime, // Showtime start time
                Items = new List<CheckoutSeatItem>() // List of selected seats
            };

            // Create a map from SeatId to "Row-SeatNumber" format
            var seatLabels = await _context.Seats
                .Where(s => seatIds.Contains(s.SeatId))
                .ToDictionaryAsync(
                    s => s.SeatId,
                    s => $"{s.RowNumber}-{s.SeatNumber}"
                );

            ViewBag.SeatLabels = seatLabels; // Pass seat labels to the view

            // Loop through each selected seat
            for (int i = 0; i < seatIds.Count; i++)
            {
                string type = ticketTypes[seatIds[i]]; // Get ticket type for this seat
                bool isStudent = type.Equals("Student", StringComparison.OrdinalIgnoreCase); // Check if ticket type is student

                // Call SQL scalar function to calculate ticket price
                decimal price = _context.Database
                    .SqlQueryRaw<decimal>(
                        "SELECT dbo.fn_CalculateTicketPrice({0}, {1}) AS Value",
                        showtime.TicketBasePrice,
                        isStudent ? 1 : 0
                    )
                    .First();

                // Add seat item to checkout model
                model.Items.Add(new CheckoutSeatItem
                {
                    SeatId = seatIds[i],
                    TicketType = type,
                    Price = price
                });

                total += price; // Add price to total amount
            }

            model.Total = total;

            // Store total amount in TempData using invariant culture
            TempData["TotalAmount"] = total.ToString(CultureInfo.InvariantCulture);
            // Store individual seat prices
            TempData["Prices"] = string.Join(",",
                model.Items.Select(x => x.Price.ToString(CultureInfo.InvariantCulture)));

            TempData.Keep(); // Preserve TempData for next request
            return View(model); // Return checkout view
        }

        // STEP 4: COMPLETE PAYMENT
        // Save booking and payment information
        [HttpPost]
        public async Task<IActionResult> CompletePayment()
        {
            TempData.Keep(); // Preserve TempData values

            // If user is not logged in, redirect to login
            if (CurrentUserId == 0)
                return RedirectToAction("Login", "Account");

            int showtimeId = int.Parse(TempData["ShowtimeId"].ToString()); // Parse showtime ID

            // Parse selected seat IDs
            List<int> seatIds = TempData["SeatIds"]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            // Parse ticket types
            var ticketTypes = TempData["TicketTypes"]
                .ToString()
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Split(':'))
                .ToDictionary(x => int.Parse(x[0]), x => x[1]);

            // Parse seat prices
            var prices = TempData["Prices"]
                .ToString()
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => decimal.Parse(x, CultureInfo.InvariantCulture))
                .ToList();

            // Parse total payment amount
            decimal totalAmount = decimal.Parse(
                TempData["TotalAmount"].ToString(),
                CultureInfo.InvariantCulture);

            // Create new booking entity
            var booking = new Booking
            {
                UserId = CurrentUserId, // Logged-in user
                ShowtimeId = showtimeId, // Related showtime
                BookingStatus = "Confirmed", // Booking confirmed
                TotalAmount = totalAmount, // Total payment
                CreatedAt = DateTime.Now, // Booking creation time
                PaidAt = DateTime.Now // Payment time
            };

            _context.Bookings.Add(booking); // Add booking to database
            await _context.SaveChangesAsync(); // Save booking to generate BookingId

            // Loop through seats to create booking seat records
            for (int i = 0; i < seatIds.Count; i++)
            {
                // Retrieve ShowtimeSeatId for seat
                int showtimeSeatId = await _context.ShowtimeSeats
                    .Where(x => x.ShowtimeId == showtimeId && x.SeatId == seatIds[i])
                    .Select(x => x.ShowtimeSeatId)
                    .FirstAsync();

                // Add booking seat record
                _context.BookingSeats.Add(new BookingSeat
                {
                    BookingId = booking.BookingId,
                    ShowtimeSeatId = showtimeSeatId,
                    TicketType = ticketTypes[seatIds[i]],
                    UnitPrice = prices[i]
                });

                // Mark seat as sold
                var stSeat = await _context.ShowtimeSeats.FindAsync(showtimeSeatId);
                stSeat.Status = "Sold";
            }

            await _context.SaveChangesAsync(); // Save all booking seat changes

            return RedirectToAction("Success"); // Redirect to success page
        }

        // MY BOOKINGS (USER ONLY)
        // Display booking history for current user
        public async Task<IActionResult> MyBookings()
        {
            // Redirect to login if user is not authenticated
            if (CurrentUserId == 0)
                return RedirectToAction("Login", "Account");

            // Retrieve user's bookings with seat counts
            var bookings = await _context.Bookings
                .Where(b => b.UserId == CurrentUserId)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    Booking = b,
                    SeatCount = _context.BookingSeats.Count(x => x.BookingId == b.BookingId)
                })
                .ToListAsync();

            // Map query result to view model
            var model = bookings
                .Select(x => new BookingHistoryViewModel
                {
                    BookingId = x.Booking.BookingId,
                    CreatedAt = x.Booking.CreatedAt,
                    TotalAmount = x.Booking.TotalAmount,
                    SeatCount = x.SeatCount
                })
                .ToList();

            return View(model); // Return booking history view
        }

        // BOOKING DETAILS
        // Display details of a specific booking
        public async Task<IActionResult> BookingDetails(int id)
        {
            // Redirect to login if user is not authenticated
            if (CurrentUserId == 0)
                return RedirectToAction("Login", "Account");

            // Retrieve booking belonging to the current user
            var booking = await _context.Bookings
                .Where(b => b.BookingId == id && b.UserId == CurrentUserId)
                .FirstOrDefaultAsync();

            // Return 404 if booking not found
            if (booking == null)
                return NotFound();

            // Retrieve related showtime
            var showtime = await _context.Showtimes
                .Where(s => s.ShowtimeId == booking.ShowtimeId)
                .FirstOrDefaultAsync();

            // Retrieve related hall
            var hall = await _context.Halls
                .Where(h => h.HallId == showtime.HallId)
                .FirstOrDefaultAsync();

            // Retrieve related movie
            var movie = await _context.Movies
                .Where(m => m.MovieId == showtime.MovieId)
                .FirstOrDefaultAsync();

            // Retrieve detailed seat information
            var seatDetails = await _context.BookingSeats
                .Where(x => x.BookingId == booking.BookingId)
                .Include(x => x.ShowtimeSeat)
                .ThenInclude(ss => ss.Seat)
                .Select(x => new
                {
                    Row = x.ShowtimeSeat.Seat.RowNumber,
                    Number = x.ShowtimeSeat.Seat.SeatNumber,
                    x.TicketType,
                    x.UnitPrice
                })
                .ToListAsync();

            // Build booking details view model
            var model = new BookingDetailsViewModel
            {
                BookingId = booking.BookingId,
                MovieName = movie.Title,
                HallName = hall.Name,
                StartTime = showtime.StartTime,
                Seats = seatDetails.Select(s => $"{s.Row}-{s.Number}").ToList(),
                TotalAmount = booking.TotalAmount,
                Tickets = seatDetails.Select(s => $"{s.TicketType} ({s.UnitPrice} ₺)").ToList()
            };

            return View(model); // Return booking details view
        }

        // SUCCESS PAGE
        // Display payment success page
        [AllowAnonymous]
        public IActionResult Success()
        {
            return View(); // Return success view
        }
    }
}




















