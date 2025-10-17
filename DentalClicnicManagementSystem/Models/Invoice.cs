using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Models
{
   

    public enum PaymentStatus
    {
        Pending,
        Paid,
        PartiallyPaid,
        Overdue,
        Cancelled,
        InsurancePending,
        Completed
    }
    // Update Invoice Model
   
    // Invoice Type Enum
    public enum InvoiceType
    {
        Appointment = 1,
        Treatment = 2,
        Laboratory = 3,
        Medication = 4,
        Combined = 5,
        General = 6// For appointment + treatment in single invoice
    }
    
    // Bill Receipt Model
    public class BillReceipt
    {
        public int Id { get; set; }
        public string ReceiptNumber { get; set; }
        public int InvoiceId { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? Notes { get; set; }

        // Navigation
        public Invoice? Invoice { get; set; }

        // Audit
        public DateTimeOffset CreatedAt { get; set; } = DateTime.Now;
        public string? CreatedBy { get; set; }
        public string? TransactionReference { get; internal set; }
    }


    // Updated Invoice Model
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

      
        public string? InvoiceNumber { get; set; } = string.Empty;

        public InvoiceType InvoiceType { get; set; } = InvoiceType.Appointment;

        [DataType(DataType.Date)]
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        // Foreign Keys for different invoice types
        public int? AppointmentId { get; set; }
        public int? PatientId { get; set; }
        public int? DoctorId { get; set; }
        public int? LabTestOrderId { get; set; }

        // Customer Info
        [Required, MaxLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [EmailAddress, MaxLength(200)]
        public string CustomerEmail { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? CustomerAddress { get; set; } = string.Empty;

        // Financial Details
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tax { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountDue { get; set; }

        // Payment Details
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        [ValidateNever]
        public PaymentMethod PaymentMethod { get; set; }

        // Currency
        [Required, MaxLength(3)]
        public string CurrencyCode { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRate { get; set; } = 1m;

        // Additional Info
        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public string status { get; set; } = "Pending"; // Active, Cancelled, Refunded

        // Navigation Properties
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public int? PaymentMethodId { get; set; }
        public Appointment? Appointment { get; set; }
        public LabTestOrder? LabTestOrder { get; set; }
        public List<InvoiceItem> Items { get; set; } = new();
        public List<InvoicePayment> Payments { get; set; } = new();
        public bool IsAppointmentInvoice { get; internal set; }
        [ValidateNever]
        public string? UpdatedBy { get; set; }
        public int LaboratoryOrderId { get;  set; }
        [ValidateNever]
        public string CreatedBy { get; internal set; }
        public bool IsCombinedInvoice { get; internal set; }
        public bool IsMedicationInvoice { get; internal set; }
        public bool IsLaboratoryInvoice { get; internal set; }
    }

    // New Payment tracking model
    public class InvoicePayment
    {
        [Key]
        public int Id { get; set; }

        public int InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        public int PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        [MaxLength(100)]
        public string? TransactionReference { get; set; }

        public DateTimeOffset PaymentDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notes { get; set; }
        public string? CreatedBy { get; internal set; }
    }

    // Lab Test Order model (if not exists)
    public class LabTestOrder
    {
        [Key]
        public int Id { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        [Required]
        public string TestName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
       

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletionDate { get; set; }

        public string Status { get; set; } = "Ordered"; // Ordered, InProgress, Completed, Cancelled

        [MaxLength(1000)]
        public string? Instructions { get; set; }
    }
   
    public class InvoiceItem
    {
        [Key]
        public int Id { get; set; }
        public int InvoiceId { get; set; }
       
        public Invoice Invoice { get; set; }
        public string ItemType { get; set; }


        public string? Description { get; set; } 
        public string? TreatmentName { get; set; } 
        public string? AppointmentWIth { get; set; } 
        public string? MedicationsName { get; set; } 

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

   
        [NotMapped]
        public decimal LineTotal => Quantity * UnitPrice;

   
        public int? InventoryItemId { get; set; }
        public int? TreatmentId { get; set; }
        public Treatment? Treatment { get; set; }
        public int? LabTestId { get; set; }
        public InventoryItem? InventoryItem { get; set; }
        public Appointment? Appointment { get; set; }
        public int? AppointmentId { get; internal set; }
        public string? TestName { get; set; }
        public int? MedicationsId { get; internal set; }
        [ValidateNever]
        public Medications Medications { get; internal set; }
        public decimal Total { get; internal set; }
        [NotMapped]
        public string? ItemDisplayName { get; set; }
        [NotMapped]
        public string? DisplayName =>
       ItemType switch
       {
           "Consultation" => AppointmentWIth,
           "Treatment" => TreatmentName,
           "Test" => TestName,
           "Medication" or "Inventory" => MedicationsName ?? InventoryItem?.Name,
           _ => Description
       };
    }
}
