// Snack.cs
// Purpose: Represents a snack/buffet product

namespace CinemaAutomation.Web.Data.Entities // Namespace containing entity classes
{
    public class Snack // Entity representing a snack item sold at the buffet
    {
        public int SnackId { get; set; } // Primary key of the Snack table
        public string Name { get; set; } = null!; // Name of the snack product
        public decimal UnitPrice { get; set; } // Unit price of the snack
        public int StockQuantity { get; set; } // Available stock quantity
        public bool IsActive { get; set; } // Indicates whether the snack is active/available for sale
    }
}


