using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DentalClicnicManagementSystem.Models
{
    [Index(nameof(SKU), IsUnique = true)]
    public class InventoryItem
    {
        public int InventoryItemId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string SKU { get; set; } = string.Empty;

        public int QtyOnHand { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int MinStock { get; set; }

        // Navs
        public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    }
}
