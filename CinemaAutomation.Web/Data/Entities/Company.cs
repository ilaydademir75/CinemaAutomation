// Company.cs
// Purpose: Represents a cinema operating company entity

// Define the namespace for entity classes
namespace CinemaAutomation.Web.Data.Entities
{
    // Entity class that represents a cinema company
    public class Company
    {
        // Primary key that uniquely identifies the company
        public int CompanyId { get; set; }

        // Name of the cinema company
        public string Name { get; set; } = null!;

        // Contact email address of the company (optional)
        public string? ContactEmail { get; set; }

        // Contact phone number of the company (optional)
        public string? Phone { get; set; }

        // Date and time when the company record was created
        public DateTime CreatedAt { get; set; }
    }
}

