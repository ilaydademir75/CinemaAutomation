// Booking.cs
// Purpose: Represents the booking header for ticket purchases

namespace CinemaAutomation.Web.Data.Entities // Define the namespace for entity classes
{
    public class Booking // Entity class that represents a ticket booking
    {
        public int BookingId { get; set; } // Primary key that uniquely identifies the booking
        public int UserId { get; set; } // Foreign key referencing the user who made the booking
        public int ShowtimeId { get; set; } // Foreign key referencing the related showtime
        public string BookingStatus { get; set; } = null!; // Current status of the booking (Pending, Confirmed, Cancelled)
        public decimal TotalAmount { get; set; } // Total price of all tickets in this booking
        public DateTime CreatedAt { get; set; } // Date and time when the booking was created
        public DateTime? PaidAt { get; set; } // Date and time when the booking was paid (null if not paid yet)
    }
}

