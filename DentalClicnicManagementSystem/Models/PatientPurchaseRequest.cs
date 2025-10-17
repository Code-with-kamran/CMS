namespace CMS.Models
{
    public class PatientPurchaseRequest
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public List<PurchaseItem> Items { get; set; } = new();
    }
}
