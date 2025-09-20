namespace CMS.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public string Transaction { get; set; }
        public decimal Total { get; set; }
        public int PatientId { get; set; } // Foreign key to Patient
        public Patient Patient { get; set; } // Navigation property
    }
}
