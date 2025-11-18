// File: ViewModels/HRDashboardViewModel.cs
using CMS.Models;

namespace CMS.ViewModels
{
    public class HRDashboardViewModel
    {
        public User HRUser { get; set; } = new User();

        // Dashboard Stats
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int PresentToday { get; set; }
        public int AbsentToday { get; set; }
        public int LateToday { get; set; }
        public int OnLeaveToday { get; set; }
        public decimal TotalPayrollThisMonth { get; set; }
        public decimal AveragePerformanceRating { get; set; }

        // Percentage Changes (vs last week)
        public decimal EmployeeGrowthChange { get; set; }
        public decimal AttendanceChange { get; set; }
        public decimal PayrollChange { get; set; }
        public decimal PerformanceChange { get; set; }

        // Lists
        public List<Employee> RecentEmployees { get; set; } = new();
        public List<AttendanceRecord> TodayAttendance { get; set; } = new();
        public List<PerformanceReview> RecentReviews { get; set; } = new();
        public List<Employee> LowLeaveBalanceEmployees { get; set; } = new();
    }
}
