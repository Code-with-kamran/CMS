using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Models
{
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Display name of the inventory item.
        /// </summary>
        [Required(ErrorMessage = "Item Name is required."), MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Stock Keeping Unit (unique business code).
        /// </summary>
        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        /// <summary>
        /// Current available quantity in stock.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative number.")]
        public int Stock { get; set; }

        /// <summary>
        /// Price per unit in local currency.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be a non-negative number.")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Category label (defaults to General).
        /// </summary>
        [MaxLength(100)]
        public string? Category { get; set; } = "General";

        [MaxLength(1000)]
        public string? Description { get; set; }

        // Note: 'Price' and 'Quantity' fields seem redundant if 'UnitPrice' and 'Stock' are used for inventory.
        // I'll assume 'UnitPrice' and 'Stock' are the primary ones for inventory.
        // If 'Price' and 'Quantity' are for something else (e.g., a specific transaction line item),
        // please clarify. For now, I'll keep them as per your model but focus on UnitPrice and Stock for inventory.
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; } // Potentially redundant with UnitPrice for inventory
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } // Potentially redundant with Stock for inventory

        public bool IsActive { get; set; } = true;
    }
}
