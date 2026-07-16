// Seat.cs
// Purpose: Represents a physical seat inside a cinema hall

namespace CinemaAutomation.Web.Data.Entities // Namespace for entity classes used in the application
{
    public class Seat // Entity representing a single seat
    {
        public int SeatId { get; set; } // Primary key of the Seat table
        public int HallId { get; set; } // Foreign key referencing the related hall
        public int RowNumber { get; set; } // Row number where the seat is located
        public int SeatNumber { get; set; } // Seat number within the row
        public bool IsActive { get; set; } // Indicates whether the seat is active/usable

        public virtual ICollection<ShowtimeSeat> ShowtimeSeats { get; set; }
            = new List<ShowtimeSeat>(); // Navigation property for showtime-seat relationships
    }
}


