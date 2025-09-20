using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class DoctorDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DoctorDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var doctorId = GetCurrentDoctorId(); // Your auth logic
        var doctor = await _context.Doctors.FindAsync(doctorId);

        var viewModel = new DoctorDashboardViewModel
        {
            Doctor = doctor
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> GetData()
    {
        var doctorId = GetCurrentDoctorId();

        // Today's stats
        var today = DateTime.Today;
        var weekAgo = today.AddDays(-7);
        var previousWeek = today.AddDays(-14);

        // Current week appointments
        var totalAppointmentsToday = await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == today)
            .CountAsync();

        var onlineConsultationsToday = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate.Date == today &&
                       a.AppointmentType == "Online")
            .CountAsync();

        var cancelledAppointmentsToday = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate.Date == today &&
                       a.Status == "Cancelled")
            .CountAsync();

        // Previous week for comparison
        var totalAppointmentsPrevWeek = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate >= previousWeek &&
                       a.AppointmentDate < weekAgo)
            .CountAsync();

        var onlineConsultationsPrevWeek = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate >= previousWeek &&
                       a.AppointmentDate < weekAgo &&
                       a.AppointmentType == "Online")
            .CountAsync();

        var cancelledAppointmentsPrevWeek = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate >= previousWeek &&
                       a.AppointmentDate < weekAgo &&
                       a.Status == "Cancelled")
            .CountAsync();

        // Calculate percentage changes
        var totalAppointmentsChange = CalculatePercentageChange(totalAppointmentsToday, totalAppointmentsPrevWeek);
        var onlineConsultationsChange = CalculatePercentageChange(onlineConsultationsToday, onlineConsultationsPrevWeek);
        var cancelledAppointmentsChange = CalculatePercentageChange(cancelledAppointmentsToday, cancelledAppointmentsPrevWeek);

        // Weekly chart data (last 7 days)
        var weeklyAppointmentData = new List<int>();
        var weeklyOnlineConsultationData = new List<int>();
        var weeklyCancelledData = new List<int>();

        for (int i = 6; i >= 0; i--)
        {
            var date = today.AddDays(-i);

            var dayTotal = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date)
                .CountAsync();

            var dayOnline = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate.Date == date &&
                           a.AppointmentType == "Online")
                .CountAsync();

            var dayCancelled = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate.Date == date &&
                           a.Status == "Cancelled")
                .CountAsync();

            weeklyAppointmentData.Add(dayTotal);
            weeklyOnlineConsultationData.Add(dayOnline);
            weeklyCancelledData.Add(dayCancelled);
        }

        // Next patient
        var nextPatient = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate >= DateTime.Now &&
                       a.Status != "Cancelled")
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new PatientDetailsViewModel
            {
                PatientId = a.Patient.PatientId,
                FullName = a.Patient.FirstName,
                ProfileImage = a.Patient.ProfileImageUrl ?? "img/profiles/default-avatar.jpg",
                Address = a.Patient.Address,
                DateOfBirth = (DateTime)a.Patient.DateOfBirth,
                Gender = a.Patient.Gender,
                //Weight = a.Patient.Weight,
                //Height = a.Patient.Height,
                LastAppointmentDate = a.AppointmentDate,
                RegisterDate = a.Patient.CreatedDate,
                PhoneNumber = a.Patient.PhoneNumber
            })
            .FirstOrDefaultAsync();

        return Json(new
        {
            totalAppointmentsToday,
            onlineConsultationsToday,
            cancelledAppointmentsToday,
            totalAppointmentsChange,
            onlineConsultationsChange,
            cancelledAppointmentsChange,
            weeklyAppointmentData,
            weeklyOnlineConsultationData,
            weeklyCancelledData,
            nextPatient
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentAppointments()
    {
        var doctorId = GetCurrentDoctorId();

        var recentAppointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId)
            .OrderByDescending(a => a.AppointmentDate)
            .Take(5)
            .Select(a => new
            {
                patientName = a.Patient.FirstName,
                patientImage = a.Patient.ProfileImageUrl ?? "img/profiles/default-avatar.jpg",
                phoneNumber = a.Patient.PhoneNumber,
                appointmentDate = a.AppointmentDate.ToString("dd MMM yyyy - hh:mm tt"),
                mode = a.AppointmentType,
                status = a.Status,
                consultationFees = a.Fee
            })
            .ToListAsync();

        return Json(recentAppointments);
    }

    [HttpGet]
    public async Task<IActionResult> GetStatistics()
    {
        var doctorId = GetCurrentDoctorId();
        var lastWeek = DateTime.Today.AddDays(-7);

        var totalPatients = await _context.Appointments
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync();

        var videoConsultations = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate >= lastWeek &&
                       a.AppointmentType == "Video")
            .CountAsync();

        var rescheduled = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate >= lastWeek &&
                       a.Status == "Rescheduled")
            .CountAsync();

        var preVisitBookings = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate >= lastWeek &&
                       a.AppointmentType == "PreVisit")
            .CountAsync();

        var walkinBookings = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate >= lastWeek &&
                       a.AppointmentType == "WalkIn")
            .CountAsync();

        var followUps = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate >= lastWeek &&
                       a.AppointmentType == "FollowUp")
            .CountAsync();

        return Json(new
        {
            totalPatients,
            videoConsultations,
            rescheduled,
            preVisitBookings,
            walkinBookings,
            followUps
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetTopPatients()
    {
        var doctorId = GetCurrentDoctorId();

        var topPatients = await _context.Appointments
            .Where(a => a.DoctorId == doctorId)
            .GroupBy(a => new { a.PatientId, a.Patient.FirstName, a.Patient.ProfileImageUrl, a.Patient.PhoneNumber })
            .Select(g => new
            {
                patientName = g.Key.FirstName,
                patientImage = g.Key.ProfileImageUrl ?? "img/profiles/default-avatar.jpg",
                phoneNumber = g.Key.PhoneNumber,
                appointmentCount = g.Count()
            })
            .OrderByDescending(x => x.appointmentCount)
            .Take(5)
            .ToListAsync();

        return Json(topPatients);
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailability()
    {
        var doctorId = GetCurrentDoctorId();

        var availability = await _context.DoctorWeeklyAvailabilities
            .Where(s => s.DoctorId == doctorId)
            .Select(s => new
            {
                dayOfWeek = s.DayOfWeek,
                startTime = s.StartTime.ToString(@"hh\:mm"),
                endTime = s.EndTime.ToString(@"hh\:mm"),
                isAvailable = s.IsWorkingDay
            })
            .ToListAsync();

        return Json(availability);
    }

    [HttpGet]
    public async Task<IActionResult> GetAppointmentStatistics()
    {
        var doctorId = GetCurrentDoctorId();
        var currentMonth = DateTime.Today.Month;
        var currentYear = DateTime.Today.Year;

        var completed = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate.Month == currentMonth &&
                       a.AppointmentDate.Year == currentYear &&
                       a.Status == "Completed")
            .CountAsync();

        var pending = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate.Month == currentMonth &&
                       a.AppointmentDate.Year == currentYear &&
                       a.Status == "Pending")
            .CountAsync();

        var cancelled = await _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                       a.AppointmentDate.Month == currentMonth &&
                       a.AppointmentDate.Year == currentYear &&
                       a.Status == "Cancelled")
            .CountAsync();

        return Json(new { completed, pending, cancelled });
    }

    private decimal CalculatePercentageChange(int current, int previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round(((decimal)(current - previous) / previous) * 100, 1);
    }

    private int GetCurrentDoctorId()
    {
        // Your authentication logic to get current doctor ID
        return int.Parse(User.FindFirst("DoctorId")?.Value ?? "1");
    }


    [HttpGet]
    public async Task<IActionResult> GetAppointmentsChart(string filter = "monthly")
    {
        var doctorId = GetCurrentDoctorId();
        DateTime startDate;
        List<string> categories;

        switch (filter.ToLower())
        {
            case "weekly":
                startDate = DateTime.Today.AddDays(-7);
                categories = Enumerable.Range(0, 7)
                    .Select(i => DateTime.Today.AddDays(-6 + i).ToString("ddd"))
                    .ToList();
                break;
            case "yearly":
                startDate = DateTime.Today.AddYears(-1);
                categories = Enumerable.Range(0, 12)
                    .Select(i => DateTime.Today.AddMonths(-11 + i).ToString("MMM yyyy"))
                    .ToList();
                break;
            default: // monthly
                startDate = DateTime.Today.AddMonths(-1);
                categories = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                break;
        }

        var totalAppointments = await GetAppointmentDataForChart(doctorId, startDate, filter, null);
        var completedAppointments = await GetAppointmentDataForChart(doctorId, startDate, filter, "Completed");

        return Json(new
        {
            totalAppointments,
            completedAppointments,
            categories
        });
    }

    private async Task<List<int>> GetAppointmentDataForChart(int doctorId, DateTime startDate, string filter, string status)
    {
        var query = _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate >= startDate);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(a => a.Status == status);
        }

        switch (filter.ToLower())
        {
            case "weekly":
                return await query
                    .GroupBy(a => a.AppointmentDate.Date)
                    .Select(g => new { Date = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Date)
                    .Select(x => x.Count)
                    .ToListAsync();

            case "yearly":
                return await query
                    .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                    .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                    .OrderBy(x => x.Year).ThenBy(x => x.Month)
                    .Select(x => x.Count)
                    .ToListAsync();

            default: // monthly
                return await query
                    .GroupBy(a => a.AppointmentDate.Month)
                    .Select(g => new { Month = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Month)
                    .Select(x => x.Count)
                    .ToListAsync();
        }
    }

}
