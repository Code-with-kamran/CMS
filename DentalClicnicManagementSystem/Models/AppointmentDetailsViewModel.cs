using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMS.ViewModels
{

    // ViewModels/AppointmentDetailsViewModel.cs
    public class AppointmentDetailsViewModel
    {
        public int AppointmentId { get; set; }
        public PatientInfoViewModel Patient { get; set; } = new();
        public UpdatePatientVitalsViewModel Vitals { get; set; } = new();
        public string CurrentStatus { get; set; } = "InProgress";
        public List<PatientTreatmentViewModel> Treatments { get; set; } = new();
        public List<MedicationViewModel> Medications { get; set; } = new();     
        public List<SelectListItem> TreatmentOptions { get; set; } = new();
        public List<SelectListItem> MedicationOptions { get; set; } = new();
    }

  
    public class UpdatePatientVitalsViewModel
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
    }

    public class PatientTreatmentViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        
        public decimal UnitPrice { get; set; } = decimal.Zero;
    }

    public class MedicationViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
       
        public decimal UnitPrice { get; set; } = decimal.Zero;
    }



    public class TestVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
       
        public decimal UnitPrice { get; set; }
    }
}
