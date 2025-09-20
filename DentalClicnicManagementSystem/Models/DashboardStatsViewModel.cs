// Create this ViewModel class in a ViewModels folder
namespace CMS.ViewModels
{
    public class DashboardStatsViewModel
    {
        public int DoctorCount { get; set; }
        public int PatientCount { get; set; }
        public int AppointmentCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
