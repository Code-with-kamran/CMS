namespace CMS.ViewModels
{
    public class ThankYouViewModel
    {
        public int AppointmentId { get; set; }
        public string AppointmentNo { get; set; } = "";
        public string PatientName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public string DoctorSpecialty { get; set; } = "";
        public DateTimeOffset AppointmentDate { get; set; }
        public string TimeRange { get; set; } = "";
        public decimal Fee { get; set; }
    }
}
