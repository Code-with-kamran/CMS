// File: Models/Employee.cs
using CMS.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public class Employee
    {
        public int Id { get; set; }
        [StringLength(50)] public string? Code { get; set; }
        [Required, StringLength(100)] public string FirstName { get; set; } = default!;
        [Required, StringLength(100)] public string LastName { get; set; } = default!;
        [StringLength(200)] public string? Email { get; set; }
        [StringLength(30)] public string? Phone { get; set; }
        public int? DepartmentId { get; set; }
        public Department? Department { get; set; }
        [Required]
        [StringLength(100)]
        public string Designation { get; set; }
        public DateTime HireDate { get; set; } 
        [Range(0, 100000000)] public decimal BaseSalary { get; set; }
        public bool IsActive { get; set; } = true;
        [Range(0, 365)] public int LeaveBalance { get; set; } = 14;

        public string FullName => $"{FirstName} {LastName}";

        // Navigation properties
        [ValidateNever]
        public ICollection<AttendanceRecord> AttendanceRecords { get; set; }
        [ValidateNever]
        public ICollection<LeaveRequest> LeaveRequests { get; set; }
        [ValidateNever]
        public ICollection<PayrollItem> PayrollItems { get; set; }

        [ValidateNever]
        public ICollection<PerformanceReview> PerformanceReviews { get; set; }
    }
}
