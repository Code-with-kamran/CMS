
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DentalClicnicManagementSystem.Models
{
    public class Patient
    {
        public int PatientId { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Phone, StringLength(20)]
        public string? Phone { get; set; }

        [EmailAddress, StringLength(100)]
        public string? Email { get; set; }

        [StringLength(1000)]
        public string? Allergies { get; set; }

        // Navs
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<Xray> Xrays { get; set; } = new List<Xray>();
    }
}
