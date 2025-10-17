namespace CMS.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public string? FileUrl { get; set; }
        public int? PatientId { get; set; } // Foreign key to Patient
        public Patient Patient { get; set; } // Navigation property
        public string? Description { get; internal set; }
        public string? FileSize { get; internal set; }
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public int? AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
    }
}
