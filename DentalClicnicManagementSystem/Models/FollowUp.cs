using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CMS.Models
{
    public class FollowUp
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        [ValidateNever]
        public Patient Patient { get; set; } 
        public DateTime FollowUpDate { get; set; }
        public string? Notes { get; set; }
        public string? Status { get; set; } = "Schedule";
        public int AppointmentId { get; set; }
        [ValidateNever]
        public Appointment Appointment { get; set; }

    }
}
