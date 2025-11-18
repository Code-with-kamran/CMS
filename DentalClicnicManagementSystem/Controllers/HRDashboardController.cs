// File: Controllers/HRDashboardController.cs
using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Controllers
{
    [Authorize(Roles = "HR,Admin")]
    public class HRDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var hrUserId = GetCurrentHRUserId();
            var hrUser = await _context.Users.FindAsync(hrUserId);

            var viewModel = new HRDashboardViewModel
            {
                HRUser = new User
                {
                    Id = hrUserId,
                    FullName = hrUser?.FullName ?? "HR Manager",
                    Email = hrUser?.Email ?? "",
                    PhoneNumber = hrUser?.PhoneNumber ?? "000000000"
                }
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            // Today's date range
            DateTimeOffset todayLocal = TimeZoneInfo.ConvertTime(DateTime.Today, localTimeZone);
            DateTimeOffset todayStartUtc = todayLocal.ToUniversalTime();
            DateTimeOffset todayEndUtc = todayStartUtc.AddDays(1);

            // Last week date range
            DateTimeOffset weekAgo = todayLocal.AddDays(-7);
            DateTimeOffset prevWeekStart = weekAgo.ToUniversalTime();
            DateTimeOffset prevWeekEnd = prevWeekStart.AddDays(7);

            // TODAY'S STATS
            var totalEmployees = await _context.Employees
                .Where(e => !e.IsDeleted)
                .CountAsync();

            var activeEmployees = await _context.Employees
                .Where(e => e.IsActive && !e.IsDeleted)
                .CountAsync();

            var presentToday = await _context.AttendanceRecords
                .Where(a => a.Date >= todayStartUtc && a.Date < todayEndUtc && a.Status == AttendanceStatus.Present)
                .CountAsync();

            var absentToday = await _context.AttendanceRecords
                .Where(a => a.Date >= todayStartUtc && a.Date < todayEndUtc && a.Status == AttendanceStatus.Absent)
                .CountAsync();

            var lateToday = await _context.AttendanceRecords
                .Where(a => a.Date >= todayStartUtc && a.Date < todayEndUtc && a.Status == AttendanceStatus.Late)
                .CountAsync();

            var onLeaveToday = await _context.AttendanceRecords
                .Where(a => a.Date >= todayStartUtc && a.Date < todayEndUtc && a.Status == AttendanceStatus.leave)
                .CountAsync();

            var totalPayrollThisMonth = await _context.PayrollItems
                .Include(p => p.PayrollRun)
                .Where(p => p.PayrollRun.RunAt.Month == DateTime.Now.Month &&
                           p.PayrollRun.RunAt.Year == DateTime.Now.Year)
                .SumAsync(p => p.NetPay);

            var avgPerformanceRating = await _context.PerformanceReviews
                .Where(p => p.IsActive)
                .AverageAsync(p => (decimal?)p.Rating) ?? 0;

            // PREVIOUS WEEK STATS
            var totalEmployeesPrevWeek = await _context.Employees
                .Where(e => !e.IsDeleted && e.HireDate < prevWeekEnd)
                .CountAsync();

            var presentPrevWeek = await _context.AttendanceRecords
                .Where(a => a.Date >= prevWeekStart && a.Date < prevWeekEnd && a.Status == AttendanceStatus.Present)
                .CountAsync();

            var totalPayrollPrevMonth = await _context.PayrollItems
                .Include(p => p.PayrollRun)
                .Where(p => p.PayrollRun.RunAt.Month == DateTime.Now.AddMonths(-1).Month &&
                           p.PayrollRun.RunAt.Year == DateTime.Now.AddMonths(-1).Year)
                .SumAsync(p => p.NetPay);

            var avgPerformanceRatingPrevMonth = await _context.PerformanceReviews
                .Where(p => p.IsActive && p.ReviewDate < DateTime.Now.AddMonths(-1))
                .AverageAsync(p => (decimal?)p.Rating) ?? 0;

            // Calculate percentage changes
            var employeeGrowthChange = CalculatePercentageChange(totalEmployees, totalEmployeesPrevWeek);
            var attendanceChange = CalculatePercentageChange(presentToday, presentPrevWeek / 7); // Average per day
            var payrollChange = CalculatePercentageChange((int)totalPayrollThisMonth, (int)totalPayrollPrevMonth);
            var performanceChange = CalculatePercentageChange((int)avgPerformanceRating, (int)avgPerformanceRatingPrevMonth);

            // Get today's attendance details
            var todayAttendanceDetails = await _context.AttendanceRecords
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Department)
                .Where(a => a.Date >= todayStartUtc && a.Date < todayEndUtc)
                .OrderBy(a => a.CheckIn)
                .Take(10)
                .Select(a => new
                {
                    employeeName = a.Employee.FullName,
                    department = a.Employee.Department != null ? a.Employee.Department.DepartmentName : "N/A",
                    checkIn = a.CheckIn.HasValue ? a.CheckIn.Value.ToString("hh:mm tt") : "N/A",
                    checkOut = a.CheckOut.HasValue ? a.CheckOut.Value.ToString("hh:mm tt") : "N/A",
                    status = a.Status.ToString(),
                    overtimeHours = a.OvertimeHours ?? 0
                })
                .ToListAsync();

            // Get low leave balance employees
            var lowLeaveBalanceEmployees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => e.IsActive && !e.IsDeleted && e.LeaveBalance < 5)
                .OrderBy(e => e.LeaveBalance)
                .Take(5)
                .Select(e => new
                {
                    id = e.Id,
                    name = e.FullName,
                    department = e.Department != null ? e.Department.DepartmentName : "N/A",
                    leaveBalance = e.LeaveBalance,
                    designation = e.Designation
                })
                .ToListAsync();

            // Recent employees (last 30 days)
            var recentEmployees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => !e.IsDeleted && e.HireDate >= DateTime.Now.AddDays(-30))
                .OrderByDescending(e => e.HireDate)
                .Take(5)
                .Select(e => new
                {
                    id = e.Id,
                    name = e.FullName,
                    email = e.Email,
                    department = e.Department != null ? e.Department.DepartmentName : "N/A",
                    designation = e.Designation,
                    hireDate = e.HireDate.ToString("dd MMM yyyy"),
                    baseSalary = e.BaseSalary
                })
                .ToListAsync();

            // Recent performance reviews
            var recentReviews = await _context.PerformanceReviews
                .Include(p => p.Employee)
                    .ThenInclude(e => e.Department)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.ReviewDate)
                .Take(5)
                .Select(p => new
                {
                    id = p.Id,
                    employeeName = p.Employee.FullName,
                    department = p.Employee.Department != null ? p.Employee.Department.DepartmentName : "N/A",
                    reviewer = p.Reviewer,
                    rating = p.Rating,
                    reviewDate = p.ReviewDate.ToString("dd MMM yyyy"),
                    notes = p.Notes
                })
                .ToListAsync();

            return Json(new
            {
                totalEmployees,
                activeEmployees,
                presentToday,
                absentToday,
                lateToday,
                onLeaveToday,
                totalPayrollThisMonth,
                avgPerformanceRating,
                employeeGrowthChange,
                attendanceChange,
                payrollChange,
                performanceChange,
                todayAttendanceDetails,
                lowLeaveBalanceEmployees,
                recentEmployees,
                recentReviews
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            var todayLocal = TimeZoneInfo.ConvertTime(DateTime.Today, localTimeZone);
            var weekAgo = todayLocal.AddDays(-7);
            var monthAgo = todayLocal.AddMonths(-1);

            var weekStartUtc = weekAgo.ToUniversalTime();
            var monthStartUtc = monthAgo.ToUniversalTime();

            var totalDepartments = await _context.Departments.CountAsync();

            var weeklyNewHires = await _context.Employees
                .Where(e => e.HireDate >= weekStartUtc && !e.IsDeleted)
                .CountAsync();

            var monthlyPayroll = await _context.PayrollItems
                .Include(p => p.PayrollRun)
                .Where(p => p.PayrollRun.RunAt >= monthStartUtc)
                .SumAsync(p => p.NetPay);

            var totalReviews = await _context.PerformanceReviews
                .Where(p => p.IsActive)
                .CountAsync();

            var avgAttendanceRate = await CalculateAttendanceRateAsync(weekStartUtc);

            var totalOnLeave = await _context.AttendanceRecords
                .Where(a => a.Date >= DateTime.Today && a.Status == AttendanceStatus.leave)
                .CountAsync();

            return Json(new
            {
                totalDepartments,
                weeklyNewHires,
                monthlyPayroll,
                totalReviews,
                avgAttendanceRate,
                totalOnLeave
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetQuickStats()
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            var todayLocal = TimeZoneInfo.ConvertTime(DateTime.Today, localTimeZone);
            var todayStartUtc = todayLocal.ToUniversalTime();
            var todayEndUtc = todayStartUtc.AddDays(1);

            var todaysPresent = await _context.AttendanceRecords
                .Where(a => a.Date >= todayStartUtc && a.Date < todayEndUtc && a.Status == AttendanceStatus.Present)
                .CountAsync();

            var todaysAbsent = await _context.AttendanceRecords
                .Where(a => a.Date >= todayStartUtc && a.Date < todayEndUtc && a.Status == AttendanceStatus.Absent)
                .CountAsync();

            var todaysLate = await _context.AttendanceRecords
                .Where(a => a.Date >= todayStartUtc && a.Date < todayEndUtc && a.Status == AttendanceStatus.Late)
                .CountAsync();

            var pendingReviews = await _context.PerformanceReviews
                .Where(p => p.IsActive && p.ReviewDate.Month == DateTime.Now.Month)
                .CountAsync();

            return Json(new
            {
                todaysPresent,
                todaysAbsent,
                todaysLate,
                pendingReviews
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendanceChart(string filter = "monthly")
        {
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            DateTimeOffset today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);
            DateTimeOffset startDate;
            List<string> categories;

            switch (filter.ToLower())
            {
                case "weekly":
                    startDate = today.AddDays(-6);
                    categories = BuildCategories(filter);
                    break;
                case "yearly":
                    startDate = today.AddYears(-1);
                    categories = BuildCategories(filter);
                    break;
                default: // monthly
                    startDate = today.AddMonths(-11);
                    categories = BuildCategories(filter);
                    break;
            }

            var presentCounts = await GetAttendanceCountsAsync(startDate, filter, AttendanceStatus.Present);
            var absentCounts = await GetAttendanceCountsAsync(startDate, filter, AttendanceStatus.Absent);
            var lateCounts = await GetAttendanceCountsAsync(startDate, filter, AttendanceStatus.Late);

            return Json(new
            {
                presentCounts,
                absentCounts,
                lateCounts,
                categories
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentAttendance()
        {
            var recentAttendance = await _context.AttendanceRecords
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Department)
                .OrderByDescending(a => a.Date)
                .Take(10)
                .Select(a => new
                {
                    employeeName = a.Employee.FullName,
                    department = a.Employee.Department != null ? a.Employee.Department.DepartmentName : "N/A",
                    date = a.Date.ToString("dd MMM yyyy"),
                    checkIn = a.CheckIn.HasValue ? a.CheckIn.Value.ToString("hh:mm tt") : "N/A",
                    checkOut = a.CheckOut.HasValue ? a.CheckOut.Value.ToString("hh:mm tt") : "N/A",
                    status = a.Status.ToString(),
                    overtimeHours = a.OvertimeHours ?? 0
                })
                .ToListAsync();

            return Json(recentAttendance);
        }

        [HttpGet]
        public async Task<IActionResult> GetDepartmentStats()
        {
            var departmentStats = await _context.Departments
                .Select(d => new
                {
                    departmentName = d.DepartmentName,
                    employeeCount = _context.Employees.Count(e => e.DepartmentId == d.DepartmentId && !e.IsDeleted && e.IsActive)
                })
                .Where(d => d.employeeCount > 0)
                .OrderByDescending(d => d.employeeCount)
                .Take(5)
                .ToListAsync();

            return Json(departmentStats);
        }

        // Helper Methods
        private decimal CalculatePercentageChange(int current, int previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return Math.Round(((decimal)(current - previous) / previous) * 100, 1);
        }

        private int GetCurrentHRUserId()
        {
            return int.Parse(User.FindFirst("UserId")?.Value ?? "1");
        }

        private List<string> BuildCategories(string filter)
        {
            return filter.ToLower() switch
            {
                "weekly" => Enumerable.Range(0, 7)
                                      .Select(i => DateTime.Today.AddDays(-6 + i).ToString("ddd"))
                                      .ToList(),
                "yearly" => Enumerable.Range(0, 12)
                                      .Select(i => DateTime.Today.AddMonths(-11 + i).ToString("MMM yyyy"))
                                      .ToList(),
                _ => Enumerable.Range(0, 12)
                               .Select(i => DateTime.Today.AddMonths(-11 + i).ToString("MMM yyyy"))
                               .ToList()
            };
        }

        private async Task<List<int>> GetAttendanceCountsAsync(DateTimeOffset start, string filter, AttendanceStatus status)
        {
            var query = _context.AttendanceRecords
                .Where(a => a.Date >= start && a.Status == status);

            var records = await query
                .Select(a => new { Local = a.Date })
                .ToListAsync();

            return filter.ToLower() switch
            {
                "weekly" => records
                    .GroupBy(a => a.Local.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .ToList(),
                "yearly" => records
                    .GroupBy(a => a.Local.Year)
                    .OrderBy(g => g.Key)
                    .Select(g => g.Count())
                    .ToList(),
                _ => records
                    .GroupBy(a => new { a.Local.Year, a.Local.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => g.Count())
                    .ToList()
            };
        }

        private async Task<decimal> CalculateAttendanceRateAsync(DateTimeOffset startDate)
        {
            var totalRecords = await _context.AttendanceRecords
                .Where(a => a.Date >= startDate)
                .CountAsync();

            if (totalRecords == 0) return 0;

            var presentRecords = await _context.AttendanceRecords
                .Where(a => a.Date >= startDate && a.Status == AttendanceStatus.Present)
                .CountAsync();

            return Math.Round((decimal)presentRecords / totalRecords * 100, 1);
        }
    }
}
