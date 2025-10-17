// File: Models/PerformanceReview.cs
using CMS.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class PerformanceReview
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public bool IsActive { get; set; } = true;
        [Required]
        [DataType(DataType.Date)]
        public DateTime ReviewDate { get; set; }

        [Required]
        [StringLength(255)]
        public string Reviewer { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // 1-5 scale

        [StringLength(2000)]
        public string Notes { get; set; }
    }
}
