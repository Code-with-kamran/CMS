using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        public string? AppointmentNo { get; set; } = string.Empty;
        [Required]
        public int PatientId { get; set; }
         [ValidateNever]
        public Patient Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }
        [ValidateNever]
        public Doctor Doctor { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }
        public DateTime CreatedOn { get; set; }

        // ✅ New field for Department
        public int? DepartmentId { get; set; }
        [ValidateNever]
        public Department Department { get; set; }

        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Fee { get; set; } // <-- appointment fee

        public string Status { get; set; } = "Scheduled";
        public string AppointmentType { get; set; } = "General";
        public string Mode { get; set; } = "In-Person"; // New field for Mode
        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navs
        public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
    }
}
