namespace CMS.ViewModels
{
    public class UpcomingAppointment
    {
        public string PatientName { get; set; }
        public string PatientImage { get; set; }
        public string AppointmentNumber { get; set; }
        public string ServiceType { get; set; }
        public DateTimeOffset AppointmentDate { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public int AppointmentId { get; set; } // Add this to get the appointment ID
    }
}
