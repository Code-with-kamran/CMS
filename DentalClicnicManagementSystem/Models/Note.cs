namespace CMS.Models
{
    public class Note
    {
        public int Id { get; set; }

        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int AppointmentId { get; set; }     // FK
        public Appointment Appointment { get; set; } = null!;
    }
}
