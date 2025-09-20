// File: Models/LeaveRequest.cs
using CMS.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public enum LeaveTypeEnum { Annual, Sick, Casual, Unpaid, PublicHoliday }
    public enum LeaveStatus { Pending, Approved, Rejected }

    public class LeaveRequest
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        [ValidateNever]
        public Employee? Employee { get; set; }
        public LeaveTypeEnum Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [Range(0, 365)] public int Days { get; set; }
        [StringLength(500)] public string? Reason { get; set; }
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
        [StringLength(120)] public string? DecisionBy { get; set; }
        public DateTime? DecisionAt { get; set; }
    }
}
