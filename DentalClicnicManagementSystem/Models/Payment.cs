namespace CMS.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string? TransactionReference { get; set; }
        public decimal Total { get; set; }
        public DateTimeOffset? PaymentDate { get; set; }
        public PaymentStatus? Status { get; set; }
        public int? PatientId { get; set; } // Foreign key to Patient
        public Patient? Patient { get; set; } // Navigation property
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        public int PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public string? Notes { get; internal set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation property
    }

}
