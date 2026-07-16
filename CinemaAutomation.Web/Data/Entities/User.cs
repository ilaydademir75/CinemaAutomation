// User.cs
// Purpose: Represents an application user (admin, company, customer)

namespace CinemaAutomation.Web.Data.Entities // Namespace containing entity classes
{
    public class User // Entity representing an application user
    {
        public int UserId { get; set; } // Primary key of the User table
        public string FullName { get; set; } = null!; // Full name of the user
        public string Email { get; set; } = null!; // Email address used for login and communication
        public string PasswordHash { get; set; } = null!; // Hashed password for security
        public int RoleId { get; set; } // Foreign key referencing the user's role
        public bool IsActive { get; set; } // Indicates whether the user account is active
        public DateTime CreatedAt { get; set; } // Date and time when the user account was created
        public Role? Role { get; set; } // Navigation property to the related role
    }
}

