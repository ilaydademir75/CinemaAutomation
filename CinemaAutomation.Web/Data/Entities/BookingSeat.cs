// BookingSeat.cs
// Purpose: Represents an individual seat ticket within a booking

// Define the namespace for entity classes
namespace CinemaAutomation.Web.Data.Entities
{
    // Entity class that represents a seat-level ticket in a booking
    public class BookingSeat
    {
        // Primary key that uniquely identifies the booking seat record
        public int BookingSeatId { get; set; }

        // Foreign key referencing the parent booking
        public int BookingId { get; set; }

        // Foreign key referencing the related showtime seat
        public int ShowtimeSeatId { get; set; }

        // Type of ticket purchased for this seat (Student or Full)
        public string TicketType { get; set; } = null!;

        // Price charged for this specific seat
        public decimal UnitPrice { get; set; }

        // Navigation property to access related ShowtimeSeat entity
        public ShowtimeSeat? ShowtimeSeat { get; set; }

        // Navigation property to access parent Booking entity
        public Booking? Booking { get; set; }
    }
}


