using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DentalClicnicManagementSystem.Models
{
    
        public class ApplicationUser : IdentityUser
        {
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Optional HR-specific fields
        public DateTime? HireDate { get; set; }
        public decimal? Salary { get; set; }
        public string? Department { get; set; }
    }
    
}
