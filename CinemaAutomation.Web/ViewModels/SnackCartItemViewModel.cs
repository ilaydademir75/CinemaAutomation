using CinemaAutomation.Web.Data.Entities;

namespace CinemaAutomation.Web.ViewModels
{
    public class SnackCartItemViewModel
    {
        public Snack Snack { get; set; } = null!;
        public int Quantity { get; set; }

        public decimal Subtotal => Snack.UnitPrice * Quantity;
    }
}

