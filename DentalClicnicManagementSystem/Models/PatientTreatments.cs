namespace CMS.Models
{
    public class PatientTreatments
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }
        public string? Name { get; set; }
        public decimal UnitPrice { get; set; } = decimal.Zero;
        public Appointment Appointment { get; set; } = null!;
        public int TreatmentId { get; internal set; }
        public Treatment Treatment { get; set; }

    }
}
