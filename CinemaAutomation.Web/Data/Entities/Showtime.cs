// Showtime.cs
// Purpose: Represents a movie screening session

namespace CinemaAutomation.Web.Data.Entities // Namespace containing entity classes
{
    public class Showtime // Entity representing a movie showtime
    {
        public int ShowtimeId { get; set; } // Primary key of the Showtime table

        public int MovieId { get; set; } // Foreign key referencing the Movie entity
        public Movie? Movie { get; set; } // Navigation property to the related movie

        public int HallId { get; set; } // Foreign key referencing the Hall entity
        public Hall? Hall { get; set; } // Navigation property to the related hall

        public DateTime StartTime { get; set; } // Date and time when the screening starts
        public decimal TicketBasePrice { get; set; } // Base ticket price for this showtime
        public bool IsActive { get; set; } // Indicates whether the showtime is active
    }
}




