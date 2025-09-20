namespace CMS.Models
{
    public class PatientVitals
    {
        public int Id { get; set; }
        public string? BloodPressure { get; set; }
        public string? HeartRate { get; set; }
        public string? Spo2 { get; set; }
        public string? Temperature { get; set; }
        public string? RespiratoryRate { get; set; }
        public string? Weight { get; set; }
    }
}
