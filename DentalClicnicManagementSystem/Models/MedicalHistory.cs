namespace CMS.Models
{
    public class MedicalHistory
    {
        public int Id { get; set; }
        public DateTime DateOfVisit { get; set; }
        public string Diagnosis { get; set; }
        public string Severity { get; set; } // High, Low
        public int TotalVisits { get; set; }
        public string Status { get; set; } // Under Treatment, Cured
        public Document Document { get; set; }

        // Add PatientId to link MedicalHistory to Patient
        public int PatientId { get; set; }  // This is the foreign key for the Patient

        // Navigation property to Patient (optional)
        public Patient Patient { get; set; }
    }
}
