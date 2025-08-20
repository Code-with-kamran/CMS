using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class Treatment
    {
        public int TreatmentId { get; set; }

        [Required]
        public int AppointmentId { get; set; }
        public Appointment? Appointment { get; set; }

        [StringLength(20)]
        public string? ToothCode { get; set; } // e.g., "11", "3-5"

        [StringLength(50)]
        public string? ProcedureCode { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Precision(18, 2)]
        public decimal Cost { get; set; }

        // One-to-one (optional) to InvoiceItem
        public InvoiceItem? InvoiceItem { get; set; }
    }
}
