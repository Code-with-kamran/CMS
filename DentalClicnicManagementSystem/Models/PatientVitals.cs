using System.Text.Json.Serialization;

namespace CMS.Models
{
    public class PatientVitals
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string? BloodPressure { get; set; }
        public string? HeartRate { get; set; }
        public string? Spo2 { get; set; }
        public string? Temperature { get; set; }
        public string? RespiratoryRate { get; set; }
        public string? Weight { get; set; }

        public DateTimeOffset RecordedAt { get; set; }
        [JsonIgnore]
        public Appointment? Appointment { get; set; } = null!;

        // FIX: Ignore navigation property during JSON serialization
       
    }
}
