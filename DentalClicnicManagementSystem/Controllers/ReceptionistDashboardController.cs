using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Receptionist")]
public class ReceptionistDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReceptionistDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var receptionistId = GetCurrentReceptionistId();
        var receptionist = await _context.Users.FindAsync(receptionistId);

        var viewModel = new ReceptionistDashboardViewModel
        {
            Receptionist = new User
            {
                Id = receptionistId,
                FullName = receptionist.FullName ?? "Receptionist",
                Email = receptionist?.Email ?? "",
                PhoneNumber = "000000000"
            }
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        // Get the local time zone for Pakistan
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

        // Today's date range
        DateTimeOffset todayLocal = TimeZoneInfo.ConvertTime(DateTime.Today, localTimeZone);
        DateTimeOffset todayStartUtc = todayLocal.ToUniversalTime();
        DateTimeOffset todayEndUtc = todayStartUtc.AddDays(1);

        DateTimeOffset weekAgo = todayLocal.AddDays(-7);
        DateTimeOffset prevWeekStart = weekAgo.ToUniversalTime();
        DateTimeOffset prevWeekEnd = prevWeekStart.AddDays(7);

        // TODAY'S STATS
        var totalAppointmentsToday = await _context.Appointments
            .Where(a => a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc)
            .CountAsync();

        var newPatientsToday = await _context.Patients
            .Where(p => p.CreatedDate >= todayStartUtc && p.CreatedDate < todayEndUtc)
            .CountAsync();

        var pendingAppointmentsToday = await _context.Appointments
            .Where(a => a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc && a.Status == "Pending")
            .CountAsync();

        var revenueToday = await _context.Invoices
            .Where(i => i.IssueDate >= todayStartUtc && i.IssueDate < todayEndUtc && i.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(i => i.Total);

        // PREVIOUS WEEK STATS
        var totalAppointmentsPrevWeek = await _context.Appointments
            .Where(a => a.AppointmentDate >= prevWeekStart && a.AppointmentDate < prevWeekEnd)
            .CountAsync();

        var newPatientsPrevWeek = await _context.Patients
            .Where(p => p.CreatedDate >= prevWeekStart && p.CreatedDate < prevWeekEnd)
            .CountAsync();

        var pendingAppointmentsPrevWeek = await _context.Appointments
            .Where(a => a.AppointmentDate >= prevWeekStart && a.AppointmentDate < prevWeekEnd && a.Status == "Pending")
            .CountAsync();

        var revenuePrevWeek = await _context.Invoices
            .Where(i => i.IssueDate >= prevWeekStart && i.IssueDate < prevWeekEnd && i.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(i => i.Total);

        // Calculate percentage changes
        var totalAppointmentsChange = CalculatePercentageChange(totalAppointmentsToday, totalAppointmentsPrevWeek);
        var newPatientsChange = CalculatePercentageChange(newPatientsToday, newPatientsPrevWeek);
        var pendingAppointmentsChange = CalculatePercentageChange(pendingAppointmentsToday, pendingAppointmentsPrevWeek);
        var revenueChange = CalculatePercentageChange((int)revenueToday, (int)revenuePrevWeek);

        // Weekly chart data (last 7 days)
        var weeklyAppointmentData = new List<int>();
        var weeklyNewPatientsData = new List<int>();
        var weeklyRevenueData = new List<decimal>();

        for (int i = 6; i >= 0; i--)
        {
            var date = todayLocal.AddDays(-i);
            var dateStartUtc = new DateTimeOffset(date.Date, date.Offset).ToUniversalTime();
            var dateEndUtc = dateStartUtc.AddDays(1);

            var dayAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate >= dateStartUtc && a.AppointmentDate < dateEndUtc)
                .CountAsync();

            var dayNewPatients = await _context.Patients
                .Where(p => p.CreatedDate >= dateStartUtc && p.CreatedDate < dateEndUtc)
                .CountAsync();

            var dayRevenue = await _context.Invoices
                .Where(i => i.IssueDate >= dateStartUtc && i.IssueDate < dateEndUtc && i.PaymentStatus == PaymentStatus.Paid)
                .SumAsync(i => i.Total);

            weeklyAppointmentData.Add(dayAppointments);
            weeklyNewPatientsData.Add(dayNewPatients);
            weeklyRevenueData.Add(dayRevenue);
        }

        // Get today's upcoming appointments
        var todaysAppointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc)
            .OrderBy(a => a.AppointmentDate)
            .Take(5)
            .Select(a => new
            {
                appointmentId = a.AppointmentId,
                patientName = a.Patient.FirstName + " " + (a.Patient.LastName ?? ""),
                patientImage = a.Patient.ProfileImageUrl ?? "/assets/img/patients/patient.jpg",
                doctorName = "Dr. " + a.Doctor.FullName,
                appointmentTime = TimeZoneInfo.ConvertTime(a.AppointmentDate, localTimeZone).ToString("hh:mm tt"),
                status = a.Status,
                type = a.AppointmentType
            })
            .ToListAsync();

        // Low stock inventory items
        var lowStockItems = await _context.InventoryItems
            .Where(i => i.Stock < 10 && i.IsActive)
            .OrderBy(i => i.Stock)
            .Take(5)
            .Select(i => new
            {
                id = i.Id,
                name = i.Name,
                stock = i.Stock,
                category = i.Category
            })
            .ToListAsync();

        // Recent invoices
        var recentInvoices = await _context.Invoices
            .Include(i => i.Patient)
            .OrderByDescending(i => i.IssueDate)
            .Take(5)
            .Select(i => new
            {
                id = i.Id,
                invoiceNumber = i.InvoiceNumber,
                patientName = i.CustomerName,
                amount = i.Total,
                status = i.PaymentStatus.ToString(),
                issueDate = i.IssueDate.ToString("dd MMM yyyy")
            })
            .ToListAsync();

        return Json(new
        {
            totalAppointmentsToday,
            newPatientsToday,
            pendingAppointmentsToday,
            revenueToday,
            totalAppointmentsChange,
            newPatientsChange,
            pendingAppointmentsChange,
            revenueChange,
            weeklyAppointmentData,
            weeklyNewPatientsData,
            weeklyRevenueData,
            todaysAppointments,
            lowStockItems,
            recentInvoices
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentAppointments()
    {
        var recentAppointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .OrderByDescending(a => a.AppointmentDate)
            .Take(10)
            .Select(a => new
            {
                patientName = a.Patient.FirstName + " " + (a.Patient.LastName ?? ""),
                patientImage = a.Patient.ProfileImageUrl ?? "/assets/img/patients/patient.jpg",
                doctorName = "Dr. " + a.Doctor.FullName,
                appointmentDate = TimeZoneInfo.ConvertTime(a.AppointmentDate,
                    TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")).ToString("dd MMM yyyy - hh:mm tt"),
                type = a.AppointmentType,
                status = a.Status,
                fee = a.Fee,
                phoneNumber = a.Patient.PhoneNumber
            })
            .ToListAsync();

        return Json(recentAppointments);
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

        var totalPatients = await _context.Patients.CountAsync();

        var weeklyAppointments = await _context.Appointments
            .Where(a => a.AppointmentDate >= weekStartUtc)
            .CountAsync();

        var monthlyRevenue = await _context.Invoices
            .Where(a => a.IssueDate >= monthStartUtc && a.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(a => a.Total);

        var totalDoctors = await _context.Doctors.CountAsync();

        var pendingAppointments = await _context.Appointments
            .Where(a => a.Status == "Pending")
            .CountAsync();

        var completedAppointments = await _context.Appointments
            .Where(a => a.Status == "Completed")
            .CountAsync();

        var totalInventoryItems = await _context.InventoryItems.CountAsync();
        var lowStockCount = await _context.InventoryItems.Where(i => i.Stock < 10).CountAsync();

        return Json(new
        {
            totalPatients,
            weeklyAppointments,
            monthlyRevenue,
            totalDoctors,
            pendingAppointments,
            completedAppointments,
            totalInventoryItems,
            lowStockCount
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetTodaysSchedule()
    {
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
        var todayLocal = TimeZoneInfo.ConvertTime(DateTime.Today, localTimeZone);
        var todayStartUtc = todayLocal.ToUniversalTime();
        var todayEndUtc = todayStartUtc.AddDays(1);

        var schedule = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new
            {
                appointmentId = a.AppointmentId,
                patientName = a.Patient.FirstName + " " + (a.Patient.LastName ?? ""),
                doctorName = "Dr. " + a.Doctor.FullName,
                appointmentTime = TimeZoneInfo.ConvertTime(a.AppointmentDate, localTimeZone).ToString("HH:mm"),
                status = a.Status,
                type = a.AppointmentType,
                department = a.Department.DepartmentName
            })
            .ToListAsync();

        return Json(schedule);
    }

    [HttpGet]
    public async Task<IActionResult> GetQuickStats()
    {
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
        var todayLocal = TimeZoneInfo.ConvertTime(DateTime.Today, localTimeZone);
        var todayStartUtc = todayLocal.ToUniversalTime();
        var todayEndUtc = todayStartUtc.AddDays(1);

        var todaysPending = await _context.Appointments
            .Where(a => a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc && a.Status == "Pending")
            .CountAsync();

        var todaysCompleted = await _context.Appointments
            .Where(a => a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc && a.Status == "Completed")
            .CountAsync();

        var todaysCancelled = await _context.Appointments
            .Where(a => a.AppointmentDate >= todayStartUtc && a.AppointmentDate < todayEndUtc && a.Status == "Cancelled")
            .CountAsync();

        //var unreadMessages = await _context.ContactMessages.Where(m => !m.IsRead).CountAsync();

        var pendingInvoices = await _context.Invoices
            .Where(i => i.PaymentStatus == PaymentStatus.Pending)
            .CountAsync();

        return Json(new
        {
            todaysPending,
            todaysCompleted,
            todaysCancelled,
            //unreadMessages,
            pendingInvoices
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointmentsChart(string filter = "monthly")
    {
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
        DateTimeOffset today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);
        DateTimeOffset startDate;
        List<string> categories;

        switch (filter.ToLower())
        {
            case "weekly":
                startDate = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
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

        var totalAppointments = await GetAppointmentCountsAsync(startDate, filter, null);
        var completedAppointments = await GetAppointmentCountsAsync(startDate, filter, "Completed");

        return Json(new
        {
            totalAppointments,
            completedAppointments,
            categories
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetInventoryAlerts()
    {
        var lowStockItems = await _context.InventoryItems
            .Where(i => i.Stock < 10 && i.IsActive)
            .OrderBy(i => i.Stock)
            .Select(i => new
            {
                id = i.Id,
                name = i.Name,
                stock = i.Stock,
                category = i.Category,
                unitPrice = i.UnitPrice
            })
            .ToListAsync();

        return Json(lowStockItems);
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentInvoices()
    {
        var recentInvoices = await _context.Invoices
            .Include(i => i.Patient)
            .OrderByDescending(i => i.IssueDate)
            .Take(10)
            .Select(i => new
            {
                id = i.Id,
                invoiceNumber = i.InvoiceNumber,
                patientName = i.CustomerName,
                amount = i.Total,
                status = i.PaymentStatus.ToString(),
                issueDate = i.IssueDate.ToString("dd MMM yyyy"),
                dueDate = i.DueDate.ToString("dd MMM yyyy")
            })
            .ToListAsync();

        return Json(recentInvoices);
    }

    private decimal CalculatePercentageChange(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round(((decimal)(current - previous) / previous) * 100, 1);
    }

    private int GetCurrentReceptionistId()
    {
        // Your authentication logic to get current receptionist ID
        return int.Parse(User.FindFirst("ReceptionistId")?.Value ?? "1");
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
                           .Select(i => DateTime.Today.AddMonths(-11 + i))
                           .Select(d => d.ToString("MMM yyyy"))
                           .ToList()
        };
    }

    private async Task<List<int>> GetAppointmentCountsAsync(DateTimeOffset start, string filter, string status)
    {
        var query = _context.Appointments.Where(a => a.AppointmentDate >= start);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        var appointments = await query
            .Select(a => new { Local = a.AppointmentDate })
            .ToListAsync();

        return filter.ToLower() switch
        {
            "weekly" => appointments
                .GroupBy(a => a.Local.Date)
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToList(),
            "yearly" => appointments
                .GroupBy(a => a.Local.Year)
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToList(),
            _ => appointments
                .GroupBy(a => new { a.Local.Year, a.Local.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => g.Count())
                .ToList()
        };
    }
}
