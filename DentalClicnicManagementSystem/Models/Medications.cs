namespace CMS.Models
{

    public class Medications
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal? UnitPrice { get; set; } = decimal.Zero;
    }
}
