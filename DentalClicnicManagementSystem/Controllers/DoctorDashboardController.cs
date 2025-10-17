using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Doctor")]
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

        // Get the local time zone for Pakistan
        var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
        // ------------------------------------------------------------------
        // 1.  Build once – local midnights converted to UTC
        // ------------------------------------------------------------------
        DateTimeOffset todayLocal = TimeZoneInfo.ConvertTime(DateTime.Today, localTimeZone);
        DateTimeOffset todayStartUtc = todayLocal.ToUniversalTime();          // 00:00 local → UTC
        DateTimeOffset todayEndUtc = todayStartUtc.AddDays(1);              // 24:00 local → UTC

        DateTimeOffset weekAgo = todayLocal.AddDays(-7);
        DateTimeOffset prevWeekStart = weekAgo.ToUniversalTime();             // 00:00 14 days ago UTC
        DateTimeOffset prevWeekEnd = prevWeekStart.AddDays(7);              // 00:00 7 days ago UTC
                                                                            // ------------------------------------------------------------------

        // TODAY
        var totalAppointmentsToday = await _context.Appointments
            .Where(a => a.DoctorId == doctorId
                     && a.AppointmentDate >= todayStartUtc
                     && a.AppointmentDate < todayEndUtc)
            .CountAsync();

        var completedAppointmentsToday = await _context.Appointments
            .Where(a => a.DoctorId == doctorId
                     && a.AppointmentDate >= todayStartUtc
                     && a.AppointmentDate < todayEndUtc
                     && a.Status == "Completed")
            .CountAsync();

        var cancelledAppointmentsToday = await _context.Appointments
            .Where(a => a.DoctorId == doctorId
                     && a.AppointmentDate >= todayStartUtc
                     && a.AppointmentDate < todayEndUtc
                     && a.Status == "Cancelled")
            .CountAsync();

        // PREVIOUS WEEK (7-day block)
        var totalAppointmentsPrevWeek = await _context.Appointments
            .Where(a => a.DoctorId == doctorId
                     && a.AppointmentDate >= prevWeekStart
                     && a.AppointmentDate < prevWeekEnd)
            .CountAsync();

        var completedAppointmentsPrevWeek = await _context.Appointments
            .Where(a => a.DoctorId == doctorId
                     && a.AppointmentDate >= prevWeekStart
                     && a.AppointmentDate < prevWeekEnd
                     && a.Status == "Completed")
            .CountAsync();

        var cancelledAppointmentsPrevWeek = await _context.Appointments
            .Where(a => a.DoctorId == doctorId
                     && a.AppointmentDate >= prevWeekStart
                     && a.AppointmentDate < prevWeekEnd
                     && a.Status == "Cancelled")
            .CountAsync();                                                                                                                                      

        // Calculate percentage changes
        var totalAppointmentsChange = CalculatePercentageChange(totalAppointmentsToday, totalAppointmentsPrevWeek);
        var completedAppointmentsChange = CalculatePercentageChange(completedAppointmentsToday, completedAppointmentsPrevWeek);
        var cancelledAppointmentsChange = CalculatePercentageChange(cancelledAppointmentsToday, cancelledAppointmentsPrevWeek);

        // Weekly chart data (last 7 days)
        var weeklyAppointmentData = new List<int>();
        var weeklyOnlineConsultationData = new List<int>();
        var weeklyCancelledData = new List<int>();

        for (int i = 6; i >= 0; i--)
        {
            var date = todayLocal.AddDays(-i);

            var dayTotal = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .CountAsync();

            var dayOnline = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate.Date == date.Date &&
                           a.AppointmentType == "Online")
                .CountAsync();

            var dayCancelled = await _context.Appointments
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate.Date == date.Date &&
                           a.Status == "Cancelled")
                .CountAsync();

            weeklyAppointmentData.Add(dayTotal);
            weeklyOnlineConsultationData.Add(dayOnline);
            weeklyCancelledData.Add(dayCancelled);
        }

        // Get the next patient
        PatientDetailsViewModel nextPatient = null;
        try
        {
            var nextAppointment = await _context.Appointments
                .Include(a => a.Patient) // Make sure Patient is included
                .Where(a => a.DoctorId == doctorId &&
                           a.AppointmentDate >= DateTimeOffset.UtcNow &&
                           a.Status != "Cancelled" &&
                           a.Status != "Completed") // Exclude completed as well
                .OrderBy(a => a.AppointmentDate)
                .FirstOrDefaultAsync();

            if (nextAppointment != null && nextAppointment.Patient != null)
            {
                nextPatient = new PatientDetailsViewModel
                {
                    PatientId = nextAppointment.Patient.PatientId,
                    FullName = nextAppointment.Patient.FirstName + " " + (nextAppointment.Patient.LastName ?? ""),
                    ProfileImage = nextAppointment.Patient.ProfileImageUrl ?? "assets/img/profiles/default-avatar.jpg",
                    Address = nextAppointment.Patient.Address ?? "",
                    DateOfBirth = nextAppointment.Patient.DateOfBirth ?? DateTime.MinValue,
                    Gender = nextAppointment.Patient.Gender ?? "",
                    LastAppointmentDate = nextAppointment.AppointmentDate,
                    RegisterDate = nextAppointment.Patient.CreatedDate,
                    PhoneNumber = nextAppointment.Patient.PhoneNumber ?? ""
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting next patient: {ex.Message}");
            // Log the exception but don't break the entire response
        }

        return Json(new
        {
            totalAppointmentsToday,
            completedAppointmentsToday,
            cancelledAppointmentsToday,
            totalAppointmentsChange,
            completedAppointmentsChange,
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

        var totalAppointments = await GetAppointmentCountsAsync(doctorId, startDate, filter, null, localTimeZone);
        var completedAppointments = await GetAppointmentCountsAsync(doctorId, startDate, filter, "Completed", localTimeZone);

        return Json(new
        {
            totalAppointments,
            completedAppointments,
            categories
        });
    }



    [HttpGet]
    public async Task<IActionResult> GetUpcomingAppointments(string filter = "today")
    {
        try
        {
            var doctorId = GetCurrentDoctorId();

            // Pakistan timezone
            var pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            var nowInPakistan = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, pakistanTimeZone);

            DateTimeOffset startDate, endDate;

            switch (filter.ToLower())
            {
                case "today":
                    startDate = new DateTimeOffset(nowInPakistan.Date, nowInPakistan.Offset);
                    endDate = startDate.AddDays(1).AddSeconds(-1);
                    break;

                case "weekly":
                    // Calculate start of week (Monday)
                    var daysFromMonday = (nowInPakistan.DayOfWeek - DayOfWeek.Monday + 7) % 7;
                    var startOfWeek = nowInPakistan.AddDays(-daysFromMonday).Date;
                    startDate = new DateTimeOffset(startOfWeek, nowInPakistan.Offset);
                    endDate = startDate.AddDays(7).AddSeconds(-1);
                    break;

                case "monthly":
                    var startOfMonth = new DateTime(nowInPakistan.Year, nowInPakistan.Month, 1);
                    startDate = new DateTimeOffset(startOfMonth, nowInPakistan.Offset);
                    endDate = startDate.AddMonths(1).AddSeconds(-1);
                    break;

                default:
                    startDate = new DateTimeOffset(nowInPakistan.Date, nowInPakistan.Offset);
                    endDate = startDate.AddDays(1).AddSeconds(-1);
                    break;
            }

            // Convert to UTC for database query
            var startDateUtc = startDate.ToUniversalTime();
            var endDateUtc = endDate.ToUniversalTime();

            Console.WriteLine($"Searching appointments between:");
            Console.WriteLine($"  Pakistan: {startDate} to {endDate}");
            Console.WriteLine($"  UTC: {startDateUtc} to {endDateUtc}");

            var raw = await _context.Appointments
    .Include(a => a.Patient)
    .Where(a => a.DoctorId == doctorId &&
               a.AppointmentDate >= startDateUtc &&
               a.AppointmentDate <= endDateUtc &&
               a.Status != "Cancelled" &&
               a.Status != "Completed")
    .OrderBy(a => a.AppointmentDate)
    .Take(5)                       // ← max 5
    .ToListAsync();

            var pak = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            var result = raw.Select(a =>
            {
                var local = TimeZoneInfo.ConvertTime(a.AppointmentDate, pak);
                return new
                {
                    appointmentId = a.AppointmentId,
                    patientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    patientImage = a.Patient.ProfileImageUrl ?? "/assets/img/patients/patient.jpg",
                    appointmentNo = a.AppointmentNo ?? $"AP{a.AppointmentId:D6}",
                    serviceType = a.AppointmentType ?? "General Consultation",
                    appointmentDate = local.ToString("dddd, dd MMM yyyy"),
                    appointmentTime = local.ToString("hh:mm tt"),
                    department = a.Department?.DepartmentName ?? "General Medicine",
                    status = a.Status
                };
            });

            return Json(result);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUpcomingAppointments: {ex.Message}");
            Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
            return Json(new List<object>());
        }
    }




    //[HttpGet]
    //public async Task<IActionResult> GetUpcomingAppointments(string filter = "today")
    //{
    //    try
    //    {
    //        var doctorId = GetCurrentDoctorId();

    //        // Get Pakistan timezone
    //        var pakistanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

    //        // Current time in Pakistan with proper offset
    //        var nowUtc = DateTimeOffset.UtcNow;
    //        var nowInPakistan = TimeZoneInfo.ConvertTime(nowUtc, pakistanTimeZone);

    //        DateTimeOffset startDate, endDate;

    //        switch (filter.ToLower())
    //        {
    //            case "today":
    //                // Today in Pakistan timezone
    //                var todayStartPakistan = new DateTimeOffset(nowInPakistan.Year, nowInPakistan.Month, nowInPakistan.Day, 0, 0, 0, nowInPakistan.Offset);
    //                var todayEndPakistan = todayStartPakistan.AddDays(1).AddSeconds(-1);

    //                startDate = todayStartPakistan;
    //                endDate = todayEndPakistan;
    //                break;

    //            case "weekly":
    //                // Start of week (Monday) in Pakistan timezone
    //                var startOfWeekPakistan = nowInPakistan.AddDays(-(int)nowInPakistan.DayOfWeek + (int)DayOfWeek.Monday);
    //                startOfWeekPakistan = new DateTimeOffset(startOfWeekPakistan.Year, startOfWeekPakistan.Month, startOfWeekPakistan.Day, 0, 0, 0, startOfWeekPakistan.Offset);
    //                var endOfWeekPakistan = startOfWeekPakistan.AddDays(7).AddSeconds(-1);

    //                startDate = startOfWeekPakistan;
    //                endDate = endOfWeekPakistan;
    //                break;

    //            case "monthly":
    //                // Start of month in Pakistan timezone
    //                var startOfMonthPakistan = new DateTimeOffset(nowInPakistan.Year, nowInPakistan.Month, 1, 0, 0, 0, nowInPakistan.Offset);
    //                var endOfMonthPakistan = startOfMonthPakistan.AddMonths(1).AddSeconds(-1);

    //                startDate = startOfMonthPakistan;
    //                endDate = endOfMonthPakistan;
    //                break;

    //            default:
    //                // Default to today
    //                var defaultTodayStart = new DateTimeOffset(nowInPakistan.Year, nowInPakistan.Month, nowInPakistan.Day, 0, 0, 0, nowInPakistan.Offset);
    //                var defaultTodayEnd = defaultTodayStart.AddDays(1).AddSeconds(-1);
    //                startDate = defaultTodayStart;
    //                endDate = defaultTodayEnd;
    //                break;
    //        }

    //        // Convert to UTC for database query (since your appointments are stored in UTC)
    //        var startDateUtc = startDate.ToUniversalTime();
    //        var endDateUtc = endDate.ToUniversalTime();

    //        Console.WriteLine($"Filter: {filter}");
    //        Console.WriteLine($"Start Pakistan: {startDate}");
    //        Console.WriteLine($"End Pakistan: {endDate}");
    //        Console.WriteLine($"Start UTC: {startDateUtc}");
    //        Console.WriteLine($"End UTC: {endDateUtc}");

    //        var upcomingAppointments = await _context.Appointments
    //            .Include(a => a.Patient)
    //            .Where(a => a.DoctorId == doctorId &&
    //                       a.AppointmentDate >= startDateUtc &&
    //                       a.AppointmentDate <= endDateUtc &&
    //                       a.Status != "Cancelled" &&
    //                       a.Status != "Completed")
    //            .OrderBy(a => a.AppointmentDate)
    //            .Select(a => new
    //            {
    //                AppointmentId = a.AppointmentId,
    //                PatientName = a.Patient.FirstName + " " + (a.Patient.LastName ?? ""),
    //                PatientImage = a.Patient.ProfileImageUrl ?? "/assets/img/patients/patient.jpg",
    //                AppointmentNumber = a.AppointmentNumber ?? "AP" + a.AppointmentId.ToString("D6"),
    //                ServiceType = a.AppointmentType ?? "General Consultation",
    //                AppointmentDateUtc = a.AppointmentDate, // This is DateTimeOffset in UTC
    //                Department = a.Department != null ? a.Department.DepartmentName : "General Medicine",
    //                Status = a.Status
    //            })
    //            .ToListAsync();

    //        // Convert UTC times back to Pakistan time for display
    //        var result = upcomingAppointments.Select(a => new
    //        {
    //            appointmentId = a.AppointmentId,
    //            patientName = a.PatientName,
    //            patientImage = a.PatientImage,
    //            appointmentNumber = a.AppointmentNumber,
    //            serviceType = a.ServiceType,
    //            appointmentDate = TimeZoneInfo.ConvertTime(a.AppointmentDateUtc, pakistanTimeZone).ToString("dddd, dd MMM yyyy"),
    //            appointmentTime = TimeZoneInfo.ConvertTime(a.AppointmentDateUtc, pakistanTimeZone).ToString("hh:mm tt"),
    //            department = a.Department,
    //            status = a.Status
    //        }).ToList();

    //        Console.WriteLine($"Found {result.Count} appointments for {filter}");

    //        return Json(result);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error loading upcoming appointments: {ex.Message}");
    //        Console.WriteLine($"Stack trace: {ex.StackTrace}");
    //        return Json(new List<object>());
    //    }
    //}
    #region ----------  chart helpers  ----------------------------------------

    /* build the 12 (or 7 / 12) labels that will appear under the bars */
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

            _ => /* monthly – rolling 12 months, chronological */
                 Enumerable.Range(0, 12)
                           .Select(i => DateTime.Today.AddMonths(-11 + i))
                           .Select(d => d.ToString("MMM yyyy"))
                           .ToList()
        };
    }

    // ------------------------------------------------------------------
    // 1. Re-written GetAppointmentCountsAsync
    // ------------------------------------------------------------------
    private async Task<List<int>> GetAppointmentCountsAsync(
        int doctorId,
        DateTimeOffset start,          // <-- DateTimeOffset
        string filter,
        string status,
        TimeZoneInfo tz)               // still here in case you need it later
    {
        var query = _context.Appointments
            .Where(a => a.DoctorId == doctorId &&
                        a.AppointmentDate >= start);   // DateTimeOffset comparison

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        // Materialise as DateTimeOffset – no conversion to local DateTime
        var appointments = await query
            .Select(a => new
            {
                Local = a.AppointmentDate   // keep as DateTimeOffset
            })
            .ToListAsync();

        return filter.ToLower() switch
        {
            "weekly" => appointments
                .GroupBy(a => a.Local.Date)             // DateTimeOffset.Date
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToList(),

            "yearly" => appointments
                .GroupBy(a => new { a.Local.Year })     // DateTimeOffset.Year
                .OrderBy(g => g.Key.Year)
                .Select(g => g.Count())
                .ToList(),
                
            _ => appointments                            // monthly
                .GroupBy(a => new { a.Local.Year, a.Local.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => g.Count())
                .ToList()
        };
    }


    #endregion
}
