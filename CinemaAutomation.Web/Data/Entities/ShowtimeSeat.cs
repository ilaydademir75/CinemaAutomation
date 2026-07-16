// ShowtimeSeat.cs
// Purpose: Represents seat status for a specific movie showtime

namespace CinemaAutomation.Web.Data.Entities // Namespace containing entity classes
{
    public class ShowtimeSeat // Entity representing a seat for a specific showtime
    {
        public int ShowtimeSeatId { get; set; } // Primary key of the ShowtimeSeat table
        public int ShowtimeId { get; set; } // Foreign key referencing the related showtime
        public int SeatId { get; set; } // Foreign key referencing the related seat
        public string Status { get; set; } = null!; // Current seat status (Available, Reserved, Sold)
        public DateTime? ReservedUntil { get; set; } // Expiration time for temporary reservation
        public byte[] RowVersion { get; set; } = null!; // Row version for concurrency control (rowversion/timestamp)
        public Showtime? Showtime { get; set; } // Navigation property to the related showtime
        public Seat? Seat { get; set; } // Navigation property to the related seat
    }
}


