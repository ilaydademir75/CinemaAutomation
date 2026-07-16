// SnackOrder.cs
// Purpose: Represents the snack order header information

namespace CinemaAutomation.Web.Data.Entities // Namespace containing entity classes
{
    public class SnackOrder // Entity representing a snack order (order header)
    {
        public int SnackOrderId { get; set; } // Primary key of the SnackOrder table
        public int UserId { get; set; } // Foreign key referencing the user who placed the order
        public DateTime OrderDate { get; set; } // Date and time when the order was created
        public decimal TotalAmount { get; set; } // Total amount of the snack order
    }
}


