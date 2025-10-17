using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Models
{
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }


        [Required(ErrorMessage = "Item Name is required."), MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;


        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a non-negative number.")]
        public int Stock { get; set; }

   
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit Price must be a non-negative number.")]
        public decimal UnitPrice { get; set; }

   
        [MaxLength(100)]
        public string? Category { get; set; } = "General";

        [MaxLength(1000)]
        public string? Description { get; set; }
        public DateTimeOffset? LastRestockDate { get; set; } 
        public string? SupplierName { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; } // Potentially redundant with UnitPrice for inventory
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } // Potentially redundant with Stock for inventory

        public bool IsActive { get; set; } = true;
    }
}
