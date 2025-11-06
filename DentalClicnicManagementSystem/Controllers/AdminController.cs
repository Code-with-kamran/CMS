using CMS.Data; // Your DbContext namespace
using CMS.Models;
using CMS.ViewModels; // Your ViewModels namespace
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var stats = new DashboardStatsViewModel
        {
            DoctorCount = await _context.Doctors.CountAsync(),
            PatientCount = await _context.Patients.CountAsync(),
            AppointmentCount = await _context.Appointments.Where(a => a.AppointmentDate.Month == DateTime.Now.Month).CountAsync(),
            Revenue = await _context.Invoices.Where(i => i.status != "Paid").SumAsync(i => i.Total)
        };
        return View(stats);
    }

    // In: Controllers/AdminDashboardController.cs


    // In: Controllers/AdminDashboardController.cs

    // In: Controllers/AdminDashboardController.cs

    [HttpGet]
    public JsonResult GetDashboardCardChartsData()
    {
        // This method provides dummy data for the 7-day trend charts in the cards.
        // A real implementation would query the database for the last 7 days of activity.
        var rand = new Random();
        var doctorsData = Enumerable.Range(0, 7).Select(i => rand.Next(1, 10)).ToList();
        var patientsData = Enumerable.Range(0, 7).Select(i => rand.Next(20, 50)).ToList();
        var appointmentsData = Enumerable.Range(0, 7).Select(i => rand.Next(50, 100)).ToList();
        var revenueData = Enumerable.Range(0, 7).Select(i => rand.Next(1000, 5000)).ToList();

        return Json(new { doctorsData, patientsData, appointmentsData, revenueData });
    }




    // In: Controllers/AdminDashboardController.cs (Updated)

    [HttpGet]
    public async Task<IActionResult> GetAppointmentsForDate(DateTime date)
    {
        var appointments = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate.Date == date.Date)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        return PartialView("~/Views/Shared/Partials/_AppointmentList.cshtml", appointments);
    }


    [HttpGet]
    public async Task<JsonResult> GetAppointmentStatistics(string range = "Monthly")
    {
        // This single action now handles "Monthly", "Weekly", and "Yearly"
        IQueryable<Appointment> query = _context.Appointments.Where(a => a.Status != null);

        List<string> labels = new List<string>();
        List<int> completedData = new List<int>();
        List<int> ongoingData = new List<int>();
        List<int> rescheduledData = new List<int>();

        int totalAll = 0, totalCompleted = 0, totalCancelled = 0, totalRescheduled = 0;

        switch (range)
        {
            case "Weekly":
                var today = DateTime.Today;
                // Use Sunday as the start of the week, consistent with many calendars
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(6);
                labels = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i).ToString("ddd")).ToList(); // "Sun", "Mon", ...

                var weeklyStats = await query
                    .Where(a => a.AppointmentDate.Date >= startOfWeek && a.AppointmentDate.Date <= endOfWeek)
                    .ToListAsync(); // Fetch data for the week

                for (int i = 0; i < 7; i++)
                {
                    var day = startOfWeek.AddDays(i);
                    completedData.Add(weeklyStats.Count(a => a.AppointmentDate.Date == day && (a.Status == "Completed" || a.Status == "Checked Out")));
                    ongoingData.Add(weeklyStats.Count(a => a.AppointmentDate.Date == day && (a.Status == "Confirmed" || a.Status == "Scheduled")));
                    rescheduledData.Add(weeklyStats.Count(a => a.AppointmentDate.Date == day && a.Status == "Rescheduled"));
                }
                totalCompleted = weeklyStats.Count(a => a.Status == "Completed" || a.Status == "Checked Out");
                totalCancelled = weeklyStats.Count(a => a.Status == "Cancelled");
                totalRescheduled = weeklyStats.Count(a => a.Status == "Rescheduled");
                totalAll = weeklyStats.Count();
                break;

            case "Yearly":
                var fiveYearsAgo = DateTime.Now.Year - 4;
                labels = Enumerable.Range(fiveYearsAgo, 5).Select(y => y.ToString()).ToList();

                var yearlyStats = await query
                    .Where(a => a.AppointmentDate.Year >= fiveYearsAgo && a.AppointmentDate.Year <= DateTime.Now.Year)
                    .GroupBy(a => a.AppointmentDate.Year)
                    .Select(g => new
                    {
                        Year = g.Key,
                        Completed = g.Count(a => a.Status == "Completed" || a.Status == "Checked Out"),
                        Ongoing = g.Count(a => a.Status == "Confirmed" || a.Status == "Schedule"),
                        Rescheduled = g.Count(a => a.Status == "Rescheduled"),
                        Cancelled = g.Count(a => a.Status == "Cancelled")
                    }).ToListAsync();

                foreach (var yearStr in labels)
                {
                    int year = int.Parse(yearStr);
                    var yearData = yearlyStats.FirstOrDefault(y => y.Year == year);
                    completedData.Add(yearData?.Completed ?? 0);
                    ongoingData.Add(yearData?.Ongoing ?? 0);
                    rescheduledData.Add(yearData?.Rescheduled ?? 0);
                }
                totalCompleted = yearlyStats.Sum(y => y.Completed);
                totalCancelled = yearlyStats.Sum(y => y.Cancelled);
                totalRescheduled = yearlyStats.Sum(y => y.Rescheduled);
                totalAll = totalCompleted + totalCancelled + totalRescheduled + yearlyStats.Sum(y => y.Ongoing);
                break;

            default: // "Monthly"
                labels = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                var currentYear = DateTime.Now.Year;
                var monthlyStats = await query
                    .Where(a => a.AppointmentDate.Year == currentYear)
                    .GroupBy(a => a.AppointmentDate.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Completed = g.Count(a => a.Status == "Completed" || a.Status == "Checked Out"),
                        Ongoing = g.Count(a => a.Status == "Confirmed" || a.Status == "Schedule"),
                        Rescheduled = g.Count(a => a.Status == "Rescheduled"),
                        Cancelled = g.Count(a => a.Status == "Cancelled")
                    }).ToListAsync();

                for (int i = 1; i <= 12; i++)
                {
                    var monthData = monthlyStats.FirstOrDefault(m => m.Month == i);
                    completedData.Add(monthData?.Completed ?? 0);
                    ongoingData.Add(monthData?.Ongoing ?? 0);
                    rescheduledData.Add(monthData?.Rescheduled ?? 0);
                }
                totalCompleted = monthlyStats.Sum(m => m.Completed);
                totalCancelled = monthlyStats.Sum(m => m.Cancelled);
                totalRescheduled = monthlyStats.Sum(m => m.Rescheduled);
                totalAll = totalCompleted + totalCancelled + totalRescheduled + monthlyStats.Sum(m => m.Ongoing);
                break;
        }

        return Json(new
        {
            totalAll,
            totalCompleted,
            totalCancelled,
            totalRescheduled,
            labels,
            completedData,
            ongoingData,
            rescheduledData
        });
    }



    [HttpGet]
    public async Task<IActionResult> GetDayAppointments(DateTime date)
    {
        var list = await _context.Appointments
                                 .Include(a => a.Patient)
                                 .Include(a => a.Doctor)
                                 .Where(a => a.AppointmentDate.Date == date.Date)
                                 .OrderBy(a => a.AppointmentDate)
                                 .ToListAsync();

        return PartialView("~/Views/Shared/Partials/_AppointmentList.cshtml", list);
    }

    // In: Controllers/AdminDashboardController.cs

    [HttpGet]
    public async Task<IActionResult> GetPopularDoctors(string range = "Weekly")
    {
        DateTime startDate;
        DateTime endDate = DateTime.Today.AddDays(1).Date; // Up to the end of today

        switch (range)
        {
            case "Monthly":
                startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                break;
            case "Yearly":
                startDate = new DateTime(DateTime.Today.Year, 1, 1);
                break;
            default: // "Weekly"
                startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                break;
        }

        var popularDoctors = await _context.Appointments
            .Where(a => a.AppointmentDate >= startDate && a.AppointmentDate < endDate && a.DoctorId != null)
            .GroupBy(a => a.Doctor)
            .Select(g => new PopularDoctorViewModel
            {
                Doctor = g.Key,
                BookingCount = g.Count()
            })
            .OrderByDescending(d => d.BookingCount)
            .Take(3)
            .ToListAsync();

        return PartialView("~/Views/Shared/Partials/_PopularDoctorsList.cshtml", popularDoctors);
    }



}

