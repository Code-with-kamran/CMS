namespace CMS.Models
{
    public class Currency
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;



        public decimal ExchangeRate { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

}
