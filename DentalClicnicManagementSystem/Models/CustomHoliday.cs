using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class CustomHoliday
    {
        [Key] public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        
        public DateTime? Date { get; set; }

        [StringLength(255)]
        public string? Reason { get; set; }

        public bool IsActive { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
