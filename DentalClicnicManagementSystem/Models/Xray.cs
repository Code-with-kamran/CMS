using System.ComponentModel.DataAnnotations;

namespace DentalClicnicManagementSystem.Models
{
    public class Xray
    {
        public int XrayId { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        [Required, StringLength(260)]
        public string FileName { get; set; } = string.Empty; // stored relative/path

        [StringLength(450)]
        public string? UploadedBy { get; set; } // store ApplicationUserId or name

        public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
    }
}
