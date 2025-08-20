using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        [Precision(18, 2)]
        public decimal Total { get; set; }

        [Precision(18, 2)]
        public decimal Paid { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        // Navs
        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    }
}
