using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DentalClicnicManagementSystem.Models
{
    [Index(nameof(TreatmentId), IsUnique = true)] // enforce 1:1 between Treatment and InvoiceItem
    public class InvoiceItem
    {
        public int InvoiceItemId { get; set; }

        [Required]
        public int InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }

        [Required]
        public int TreatmentId { get; set; }
        public Treatment? Treatment { get; set; }

        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }

        [Range(1, int.MaxValue)]
        public int Qty { get; set; }
    }
}
