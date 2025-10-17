
using CMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace CMS.ViewModels
{
    public class AppointmentUpsertVM
    {
        public Appointment Appointment { get; set; } = new Appointment();
        public string? AppointmentNo { get; set; }
        public bool ChangeSlot { get; set; }
        public List<SelectListItem> Patients { get; set; } = new();
        public List<SelectListItem> Doctors { get; set; } = new();

        // New dropdown lists
        public List<SelectListItem> AppointmentTypes { get; set; } = new();
        public List<SelectListItem> Statuses { get; set; } = new();
        // Other repeaters
        public List<DoctorWeeklyAvailability> WeeklyAvailabilities { get; set; } = new();
        public List<DoctorDateAvailability> DateAvailabilities { get; set; } = new();
        public List<DoctorEducation> Educations { get; set; } = new();
        public List<DoctorAward> Awards { get; set; } = new();
        public List<DoctorCertification> Certifications { get; set; } = new();

        public bool PrefillFiveDaysWeek { get; set; }
        public bool PrefillSevenDaysWeek { get; set; }
    
    }
    public class AppointmentViewModel
    {
        
         public int AppointmentId { get; set; }
        public DateTimeOffset AppointmentDate { get; set; }
        public string? AppointmentType { get; set; }
        public string? DoctorName { get; set; }
        public int PatientId { get; internal set; }
        public int DoctorId { get; internal set; }
    }
    // Create these DTO classes in your Models folder
    public class AppointmentDataVM
    {
        public DateTimeOffset AppointmentDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string TreatmentName { get; set; }
        public int ConsultationDurationInMinutes { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class AppointmentExportVM
    {
        public DateTime AppointmentDate { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Treatment { get; set; }
        public TimeSpan Time { get; set; }
        public int Duration { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
}
