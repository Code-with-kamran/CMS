using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        public string ? Address { get; set; }

        public DateTime? HireDate { get; set; }

        public decimal? Salary { get; set; }

        public string? Department { get; set; }
        
    }


   
    
}
