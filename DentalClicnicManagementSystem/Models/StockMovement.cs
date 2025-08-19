using System.ComponentModel.DataAnnotations;

namespace DentalClicnicManagementSystem.Models
{
    public class StockMovement
    {
        public int StockMovementId { get; set; }

        [Required]
        public int InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }

        // Positive for additions, negative for consumption
        public int Delta { get; set; }

        public StockReason Reason { get; set; } = StockReason.Use;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
