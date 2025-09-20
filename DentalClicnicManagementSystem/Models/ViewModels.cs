using Microsoft.AspNetCore.Mvc.Rendering;

namespace CMS.Models
{
    public class ViewModels
    {
        public class DoctorUpsertVM
        {
            public Doctor Doctor { get; set; } = new Doctor();
            public List<SelectListItem> Departments { get; set; } = new();

            // Repeaters (bound via indexes)
            public List<DoctorWeeklyAvailability> WeeklyAvailabilities { get; set; } = new();
            public List<DoctorDateAvailability> DateAvailabilities { get; set; } = new();
            public List<DoctorEducation> Educations { get; set; } = new();
            public List<DoctorAward> Awards { get; set; } = new();
            public List<DoctorCertification> Certifications { get; set; } = new();

            // For quick templates
            public bool PrefillFiveDaysWeek { get; set; } // if user ticks, prefill Mon–Fri 9–5 server-side on GET
            public bool PrefillSevenDaysWeek { get; set; } // Mon–Sun 9–5
        }
    }
}
