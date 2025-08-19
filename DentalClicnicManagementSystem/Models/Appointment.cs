using System.ComponentModel.DataAnnotations;

namespace DentalClicnicManagementSystem.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        [Required]
        public int DentistId { get; set; }
        public Dentist? Dentist { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        [StringLength(1000)]
        public string? Notes { get; set; }

        // Navs
        public ICollection<Treatment> Treatments { get; set; } = new List<Treatment>();
    }
}
