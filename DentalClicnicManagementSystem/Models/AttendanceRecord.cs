// File: Models/AttendanceRecord.cs
using CMS.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace CMS.Models
{
    public enum AttendanceStatus { Present, Late, Absent,leave,HalfDay }
    public class AttendanceRecord
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        [ValidateNever]
        public Employee? Employee { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly? CheckIn { get; set; }
        public TimeOnly? CheckOut { get; set; }
        public AttendanceStatus Status { get; set; }
        public decimal? OvertimeHours { get; set; }
        [StringLength(500)] public string? Note { get; set; }
    }
}
