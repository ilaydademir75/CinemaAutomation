// Hall.cs
// Purpose: Represents a cinema hall (auditorium) entity

// Define the namespace for entity classes
namespace CinemaAutomation.Web.Data.Entities
{
    // Entity class that represents a physical cinema hall
    public class Hall
    {
        // Primary key that uniquely identifies the hall
        public int HallId { get; set; }

        // Foreign key referencing the company that owns this hall
        public int CompanyId { get; set; }

        // Display name of the cinema hall
        public string Name { get; set; } = null!;

        // Total number of seat rows in the hall
        public int TotalRows { get; set; }

        // Total number of seat columns in the hall
        public int TotalColumns { get; set; }

        // Indicates whether the hall is active or not
        public bool IsActive { get; set; }
    }
}


