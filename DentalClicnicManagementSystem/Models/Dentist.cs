using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DentalClicnicManagementSystem.Models
{
    [Index(nameof(LicenseNo), IsUnique = true)]
    public class Dentist
    {
        public int DentistId { get; set; }

        [Required, StringLength(50)]
        public string LicenseNo { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Specialty { get; set; }

        // Link to Identity user who logs in as this dentist
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }

        // Navs
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
