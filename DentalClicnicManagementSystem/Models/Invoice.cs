using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }
        /// <summary>
        /// Date when the invoice was issued.
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Date when the invoice is due for payment.
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        // ----------------- Customer Info -----------------

        [Required, MaxLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [EmailAddress, MaxLength(200)]
        public string CustomerEmail { get; set; } = string.Empty;

        [MaxLength(500)]
        public string CustomerAddress { get; set; } = string.Empty;

        // ----------------- Financials -----------------

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        /// <summary>
        /// Final total after tax and discount. Stored to avoid surprises if rates change later.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        // ----------------- Currency Handling -----------------

        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; } = "PKR"; // ISO 4217 format

        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; } = 1m;

        // ----------------- Metadata -----------------

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "Draft";

        public string InvoiceNumber { get; set; } = string.Empty;


        // New Foreign Keys
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }

        // Navigation Properties
        [ValidateNever]
        public Patient Patient { get; set; } = null!;
        [ValidateNever]
        public Doctor Doctor { get; set; } = null!;

        // ----------------- Navigation -----------------

        public List<InvoiceItem> Items { get; set; } = new();
        public int? AppointmentId { get; internal set; }
        public bool IsAppointmentInvoice { get; internal set; }
    }

    public class InvoiceItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Invoice))]
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        
        public string? Description { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Calculated line total (Qty × UnitPrice).
        /// Not mapped to DB since it’s derived.
        /// </summary>
        [NotMapped]
        public decimal LineTotal => Quantity * UnitPrice;

        /// <summary>
        /// Optional link to InventoryItem for stock tracking.
        /// </summary>
        public int? InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }
    }
}
