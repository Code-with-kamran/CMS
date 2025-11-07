
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class DefaultSettings
    {
        public int Id { get; set; }

        [Required]
        public string DefaultCurrency { get; set; } = string.Empty;

        [Required]
        public string DefaultPaymentMethod { get; set; } = string.Empty;

        [Required]
        public string WorkingHours { get; set; } = string.Empty;

        public bool IsSaturdayOpen { get; set; } = false;
        public bool IsSundayOpen { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }


}
