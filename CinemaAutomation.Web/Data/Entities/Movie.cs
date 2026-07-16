// Movie.cs
// Purpose: Represents a movie catalog item in the system

// Define the namespace for entity classes
namespace CinemaAutomation.Web.Data.Entities
{
    // Entity class that represents a movie
    public class Movie
    {
        // Primary key that uniquely identifies the movie
        public int MovieId { get; set; }

        // Title of the movie
        public string Title { get; set; } = null!;

        // Optional description or synopsis of the movie
        public string? Description { get; set; }

        // Duration of the movie in minutes
        public int DurationMinutes { get; set; }

        // Relative file path of the movie poster image
        public string? PosterPath { get; set; }

        // Indicates whether the movie is active (available for showtimes)
        public bool IsActive { get; set; }

        // Date and time when the movie record was created
        public DateTime CreatedAt { get; set; }
    }
}


