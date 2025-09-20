
using CMS.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace CMS.ViewModels
{
    public class AppointmentUpsertVM
    {
        public Appointment Appointment { get; set; } = new Appointment();

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
        public DateTime AppointmentDate { get; set; }
        public string? AppointmentType { get; set; }
        public string? DoctorName { get; set; }
        public int PatientId { get; internal set; }
        public int DoctorId { get; internal set; }
    }

}
