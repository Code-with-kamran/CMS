namespace CMS.Models
{
    public class ServiceItem
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
