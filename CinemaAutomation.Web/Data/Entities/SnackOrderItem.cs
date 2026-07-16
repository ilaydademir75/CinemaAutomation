// SnackOrderItem.cs
// Purpose: Represents an individual item within a snack order

namespace CinemaAutomation.Web.Data.Entities // Namespace containing entity classes
{
    public class SnackOrderItem // Entity representing a snack order line item (order detail)
    {
        public int SnackOrderItemId { get; set; } // Primary key of the SnackOrderItem table
        public int SnackOrderId { get; set; } // Foreign key referencing the related snack order
        public int SnackId { get; set; } // Foreign key referencing the ordered snack
        public int Quantity { get; set; } // Quantity of the snack ordered
        public decimal UnitPrice { get; set; } // Unit price of the snack at the time of order
    }
}


