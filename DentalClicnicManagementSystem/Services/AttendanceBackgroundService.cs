// File: Services/AttendanceBackgroundService.cs
using CMS.Data;
using CMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CMS.Services
{
    public class AttendanceBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AttendanceBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

        public AttendanceBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AttendanceBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Attendance Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;

                    // Run at 6:00 PM (18:00) or after to mark today's absentees
                    if (now.Hour >= 18)
                    {
                        await MarkAbsentEmployeesAsync(now.Date);
                    }

                    // Also check and mark previous day's absentees (safety net)
                    var yesterday = now.Date.AddDays(-1);
                    await MarkAbsentEmployeesAsync(yesterday);

                    // Wait for next check
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Attendance Background Service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 min on error
                }
            }

            _logger.LogInformation("Attendance Background Service is stopping.");
        }

        private async Task MarkAbsentEmployeesAsync(DateTime date)
        {
            // Skip weekends (Saturday = 6, Sunday = 0)
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                _logger.LogInformation($"Skipping attendance marking for weekend date: {date:yyyy-MM-dd}");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            try
            {
                // Get all active employees
                var activeEmployees = await context.Employees
                    .Where(e => e.IsActive && !e.IsDeleted)
                    .ToListAsync();

                // Get employees who already have attendance for this date
                var employeesWithAttendance = await context.AttendanceRecords
                    .Where(a => a.Date.Date == date.Date)
                    .Select(a => a.EmployeeId)
                    .ToListAsync();

                // Find employees without attendance
                var employeesWithoutAttendance = activeEmployees
                    .Where(e => !employeesWithAttendance.Contains(e.Id))
                    .ToList();

                if (employeesWithoutAttendance.Any())
                {
                    _logger.LogInformation($"Marking {employeesWithoutAttendance.Count} employees as absent for {date:yyyy-MM-dd}");

                    foreach (var employee in employeesWithoutAttendance)
                    {
                        var absentRecord = new AttendanceRecord
                        {
                            EmployeeId = employee.Id,
                            Date = date,
                            Status = AttendanceStatus.Absent,
                            Note = "Auto-marked as absent (no attendance recorded)",
                            CheckIn = null,
                            CheckOut = null,
                            OvertimeHours = 0
                        };

                        context.AttendanceRecords.Add(absentRecord);
                    }

                    await context.SaveChangesAsync();
                    _logger.LogInformation($"Successfully marked {employeesWithoutAttendance.Count} employees as absent");
                }
                else
                {
                    _logger.LogInformation($"All active employees have attendance for {date:yyyy-MM-dd}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking absent employees for date: {date:yyyy-MM-dd}");
            }
        }
    }
}
