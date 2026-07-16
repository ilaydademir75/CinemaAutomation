// Role.cs
// Purpose: Represents a user role such as Admin, Customer, or Company

// Define the namespace for entity classes
namespace CinemaAutomation.Web.Data.Entities
{
    // Entity class that defines a system role
    public class Role
    {
        // Primary key that uniquely identifies the role
        public int RoleId { get; set; }

        // Name of the role (e.g., Admin, Customer, Company)
        public string Name { get; set; } = null!;
    }
}
