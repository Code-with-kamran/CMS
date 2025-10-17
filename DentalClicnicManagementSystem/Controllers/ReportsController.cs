// File: Controllers/ReportsController.cs
using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Controllers
{
    [Authorize(Roles = "Admin,Manager,HR,Dentist")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ApplicationDbContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }

        #region Financial Reports

        // GET: Reports/DailyCollection
        public IActionResult DailyCollection()
        {
            return View();
        }

        // GET: Reports/
        // 
      
        public async Task<IActionResult> GetDailyCollectionData(DateTime? date, string paymentMethod)
        {
            try
            {
                // Use UTC date for consistent filtering
                var targetDate = date ?? DateTime.Today;
                var startDate = targetDate.Date; // Start of day
                var endDate = targetDate.Date.AddDays(1); // Start of next day

                var query = _context.Payments
                    .Include(p => p.Invoice)
                        .ThenInclude(i => i.Patient)
                    .Include(p => p.Invoice)
                        .ThenInclude(i => i.Doctor)
                    .Include(p => p.PaymentMethod) // Include PaymentMethod navigation property
                    .Where(p => p.PaymentDate != null &&
                               (p.Status == PaymentStatus.Completed || p.Status == PaymentStatus.Paid));

                // Fix date filtering - compare date part only
                query = query.Where(p => p.PaymentDate >= startDate && p.PaymentDate < endDate);

                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    query = query.Where(p => p.PaymentMethod.Name == paymentMethod);
                }

                var data = await query
                    .OrderBy(p => p.PaymentDate)
                    .Select(p => new
                    {
                        Id = p.Id,
                        InvoiceNumber = p.Invoice.InvoiceNumber,
                        PatientName = p.Invoice.Patient.FirstName + " " + p.Invoice.Patient.LastName,
                        DoctorName = p.Invoice.Doctor.FullName,
                        Amount = p.Total,
                        PaymentMethod = p.PaymentMethod.Name, // Use Name instead of object
                        PaymentDate = p.PaymentDate.Value.ToString("yyyy-MM-dd HH:mm"),
                        Notes = p.Notes ?? ""
                    })
                    .ToListAsync();

                var summary = new
                {
                    TotalAmount = data.Sum(d => d.Amount),
                    CashAmount = data.Where(d => d.PaymentMethod == "Cash").Sum(d => d.Amount),
                    CardAmount = data.Where(d => d.PaymentMethod == "Card").Sum(d => d.Amount),
                    OnlineAmount = data.Where(d => d.PaymentMethod == "Online").Sum(d => d.Amount),
                    PaymentCount = data.Count
                };

                return Json(new { data = data, summary = summary });
            }
            catch (Exception ex)
            {
                // Log the actual error for debugging
                _logger.LogError(ex, "Error in GetDailyCollectionData");
                return Json(new { data = new List<object>(), summary = new { } });
            }
        }
        // GET: Reports/OutstandingInvoices
        public IActionResult OutstandingInvoices()
        {
            return View();
        }

        // GET: Reports/GetOutstandingInvoicesData
        public async Task<IActionResult> GetOutstandingInvoicesData(string statusFilter)
        {
            try
            {
                // Assuming Invoice has IsPaid boolean instead of Status enum
                var query = _context.Invoices
                    .Include(i => i.Patient)
                    .Include(i => i.Doctor)
                    .Where(i => i.PaymentStatus != PaymentStatus.Paid && i.DueDate <= DateTime.Now);

                if (!string.IsNullOrEmpty(statusFilter))
                {
                    // Handle different status filters based on your actual model
                    if (statusFilter == "Overdue")
                    {
                        query = query.Where(i => i.DueDate < DateTime.Today);
                    }
                    else if (statusFilter == "DueToday")
                    {
                        query = query.Where(i => i.DueDate.Date == DateTime.Today);
                    }
                }

                var data = await query
                    .OrderBy(i => i.DueDate)
                    .Select(i => new
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        PatientName = i.Patient.FirstName + " " + i.Patient.LastName,
                        DoctorName = i.Doctor.FullName,
                        IssueDate = i.IssueDate.ToString("yyyy-MM-dd"),
                        DueDate = i.DueDate.ToString("yyyy-MM-dd"),
                        TotalAmount = CalculateInvoiceTotal(i), // Custom method to calculate total
                        AmountPaid = i.AmountPaid,
                        BalanceDue = CalculateInvoiceTotal(i) - i.AmountPaid,
                        Status = i.PaymentStatus == PaymentStatus.Paid ? "Paid" : i.DueDate < DateTime.Today ? "Overdue" : "Pending",
                        OverdueDays = (DateTime.Now - i.DueDate).Days
                    })
                    .ToListAsync();

                var summary = new
                {
                    TotalOutstanding = data.Sum(d => d.BalanceDue),
                    TotalInvoices = data.Count,
                    OverdueInvoices = data.Count(d => d.OverdueDays > 0),
                    TotalOverdueAmount = data.Where(d => d.OverdueDays > 0).Sum(d => d.BalanceDue)
                };

                return Json(new { data = data, summary = summary });
            }
            catch (Exception)
            {
                return Json(new { data = new List<object>(), summary = new { } });
            }
        }

        // Helper method to calculate invoice total
        private decimal CalculateInvoiceTotal(Invoice invoice)
        {
            // Assuming you have InvoiceItems or calculate total differently
            // Adjust this based on your actual model structure
            return invoice.Total;
        }

        // GET: Reports/DoctorRevenueShare
        public IActionResult DoctorRevenueShare()
        {
            return View();
        }

        // GET: Reports/GetDoctorRevenueShareData
        public async Task<IActionResult> GetDoctorRevenueShareData(DateTime? startDate, DateTime? endDate, int? doctorId)
        {
            try
            {
                var query = _context.Invoices
                    .Include(i => i.Doctor)
                    .Include(i => i.Patient)
                    .Where(i => i.PaymentStatus == PaymentStatus.Paid); // Using IsPaid instead of Status

                if (startDate.HasValue)
                    query = query.Where(i => i.IssueDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(i => i.IssueDate <= endDate.Value);
                if (doctorId.HasValue)
                    query = query.Where(i => i.DoctorId == doctorId.Value);

                var data = await query
                    .GroupBy(i => new { i.DoctorId, i.Doctor.FullName })
                    .Select(g => new
                    {
                        DoctorId = g.Key.DoctorId,
                        DoctorName = g.Key.FullName,
                        TotalRevenue = g.Sum(i => i.Total ),
                        RevenueShare = g.Sum(i => i.Total) * 0.7m, // 70% for doctor
                        AppointmentCount = g.Count(),
                        AverageRevenuePerAppointment = g.Average(i => i.Total)
                    })
                    .OrderByDescending(d => d.TotalRevenue)
                    .ToListAsync();

                return Json(new { data = data });
            }
            catch (Exception)
            {
                return Json(new { data = new List<object>() });
            }
        }

        // GET: Reports/TaxSummary
        public IActionResult TaxSummary()
        {
            return View();
        }

        #endregion

        #region Appointment Reports

        // GET: Reports/DoctorAppointmentList
        public IActionResult DoctorAppointmentList()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ExportDoctorAppointmentList(
            string startDate, string endDate, int? doctorId, string status, string exportType)
        {
            try
            {
                DateTime? start = null, end = null;
                var format = "yyyy-MM-dd";
                var culture = System.Globalization.CultureInfo.InvariantCulture;

                if (!string.IsNullOrWhiteSpace(startDate))
                {
                    if (DateTime.TryParseExact(startDate, format, culture, System.Globalization.DateTimeStyles.None, out DateTime s))
                        start = s;
                    else
                        return BadRequest("Invalid Start Date");
                }

                if (!string.IsNullOrWhiteSpace(endDate))
                {
                    if (DateTime.TryParseExact(endDate, format, culture, System.Globalization.DateTimeStyles.None, out DateTime e))
                        end = e;
                    else
                        return BadRequest("Invalid End Date");
                }

                // Ensure the end date goes to the very end of day
                DateTime? endInclusive = end.HasValue ? end.Value.AddDays(1).AddTicks(-1) : end;

                var data = await GetDoctorAppointmentListData(start, endInclusive, doctorId, status);

                if (data.Count == 0)
                    return Content("No data available for export");

                if (exportType?.ToLower() == "csv")
                    return ExportToCSV(data);

                return BadRequest("Invalid export type");
            }
            catch (Exception ex)
            {
                return BadRequest($"Export failed: {ex.Message}");
            }
        }

        public async Task<IActionResult> GetDoctorAppointmentList(DateTime? startDate, DateTime? endDate, int? doctorId, string status)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Treatments)
                    .AsQueryable();

                // Convert DateTime? to DateTimeOffset? for proper filtering
                DateTimeOffset? startDateOffset = startDate.HasValue ? new DateTimeOffset(startDate.Value) : null;
                DateTimeOffset? endDateOffset = endDate.HasValue ? new DateTimeOffset(endDate.Value) : null;

                // Apply filters with DateTimeOffset
                if (startDateOffset.HasValue && endDateOffset.HasValue)
                {
                    // To include all appointments ON the end date regardless of time,
                    // advance the endDateOffset to the VERY END of the day.
                    var start = startDateOffset.Value.Date;
                    var end = endDateOffset.Value.Date.AddDays(1).AddTicks(-1);

                    query = query.Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end);
                }
                else if (startDateOffset.HasValue)
                {
                    // Only a start date: all from that date forward
                    var start = startDateOffset.Value.Date;
                    query = query.Where(a => a.AppointmentDate >= start);
                }
                else if (endDateOffset.HasValue)
                {
                    var end = endDateOffset.Value.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(a => a.AppointmentDate <= end);
                }

                if (doctorId.HasValue)
                    query = query.Where(a => a.DoctorId == doctorId.Value);

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(a => a.Status == status);

                // Use DTO instead of anonymous type
                var appointments = await query
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.AppointmentDate.TimeOfDay)
                    .Select(a => new AppointmentDataVM // Use DTO
                    {
                        AppointmentDate = a.AppointmentDate,
                        FirstName = a.Patient.FirstName,
                        LastName = a.Patient.LastName,
                        FullName = a.Doctor.FullName,
                        TreatmentName = a.Treatments.Name,
                        ConsultationDurationInMinutes = a.Doctor.ConsultationDurationInMinutes,
                        Status = a.Status,
                        Notes = a.Notes
                    })
                    .ToListAsync();

                // Convert to final result format
                var result = appointments.Select(a => new
                {
                    appointmentDate = a.AppointmentDate.DateTime,
                    patientName = $"{a.FirstName} {a.LastName}",
                    doctorName = a.FullName,
                    treatment = a.TreatmentName,
                    time = a.AppointmentDate.DateTime.ToString("HH:mm"),
                    duration = a.ConsultationDurationInMinutes,
                    status = a.Status,
                    notes = a.Notes ?? string.Empty
                }).ToList();

                // Calculate summary
                var totalAppointments = await query.CountAsync();
                var todaysAppointments = await query.CountAsync(a => a.AppointmentDate.Date == DateTimeOffset.Now.Date);
                var doctorsScheduled = await query.Select(a => a.DoctorId).Distinct().CountAsync();
                var completedAppointments = await query.CountAsync(a => a.Status == "Completed");
                var completionRate = totalAppointments > 0 ? Math.Round((completedAppointments * 100.0) / totalAppointments, 1) : 0;

                var summary = new
                {
                    totalAppointments,
                    todaysAppointments,
                    doctorsScheduled,
                    completionRate
                };

                return Json(new { success = true, data = result, summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        private async Task<List<AppointmentExportVM>> GetDoctorAppointmentListData(DateTime? startDate, DateTime? endDate, int? doctorId, string status)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Treatments)
                .AsQueryable();

            // Convert DateTime? to DateTimeOffset? for proper filtering
            DateTimeOffset? startDateOffset = startDate.HasValue ? new DateTimeOffset(startDate.Value) : null;
            DateTimeOffset? endDateOffset = endDate.HasValue ? new DateTimeOffset(endDate.Value) : null;

            // Apply filters with DateTimeOffset
            // inclusive start of day
            var pkZone = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            if (startDate.HasValue)
            {
                var localStart = startDate.Value.Date;                 // 00:00 PKT
                var utcStart = TimeZoneInfo.ConvertTimeToUtc(localStart, pkZone);
                query = query.Where(a => a.AppointmentDate >= utcStart);
            }

            if (endDate.HasValue)
            {
                
                // 00:00 PKT of NEXT day -> covers entire 14th
                var localEnd = endDate.Value.Date.AddDays(1);
                var utcEnd = TimeZoneInfo.ConvertTimeToUtc(localEnd, pkZone);
                query = query.Where(a => a.AppointmentDate < utcEnd); // "<" is enough
            }

            if (doctorId.HasValue)
                query = query.Where(a => a.DoctorId == doctorId.Value);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            // Use the DTO class instead of anonymous type
            var appointments = await query
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.AppointmentDate.TimeOfDay)
                .Select(a => new AppointmentDataVM // Use DTO instead of anonymous type
                {
                    AppointmentDate = a.AppointmentDate,
                    FirstName = a.Patient.FirstName,
                    LastName = a.Patient.LastName,
                    FullName = a.Doctor.FullName,
                    TreatmentName = a.Treatments.Name,
                    ConsultationDurationInMinutes = a.Doctor.ConsultationDurationInMinutes,
                    Status = a.Status,
                    Notes = a.Notes
                })
                .ToListAsync();

            // Convert to final ViewModel
            var result = appointments.Select(a => new AppointmentExportVM
            {
                AppointmentDate = a.AppointmentDate.DateTime,
                PatientName = $"{a.FirstName} {a.LastName}",
                DoctorName = a.FullName,
                Treatment = a.TreatmentName,
                Time = a.AppointmentDate.DateTime.TimeOfDay,
                Duration = a.ConsultationDurationInMinutes,
                Status = a.Status,
                Notes = a.Notes ?? string.Empty
            }).ToList();

            return result;
        }
        public IActionResult IndividualDoctorAppointments()
        {
            return View();
        }

        // GET: Reports/TreatmentStatistics
        public IActionResult TreatmentStatistics()
        {
            return View();
        }

        // GET: Reports/GetTreatmentStatisticsData
        public async Task<IActionResult> GetTreatmentStatisticsData(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var query = _context.Appointments
                    .Where(a => a.Status == "Completed");

                if (startDate.HasValue)
                    query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Appointment, Doctor>)query.Where(a => a.AppointmentDate >= startDate.Value);
                if (endDate.HasValue)
                    query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Appointment, Doctor>)query.Where(a => a.AppointmentDate <= endDate.Value);

                // Group by treatment type instead of Treatment navigation property
                var data = await query
                    .GroupBy(a => new { TreatmentType = a.Treatments.Name ?? "General Checkup" })
                    .Select(g => new
                    {
                        TreatmentName = g.Key.TreatmentType,
                        Category = "Dental", // Default category
                        AppointmentCount = g.Count(),
                        TotalRevenue = g.Sum(a => a.Fee), // Use TreatmentFee if available
                        AverageDuration = g.Average(a => a.Fee)
                    })
                    .OrderByDescending(t => t.AppointmentCount)
                    .ToListAsync();

                return Json(new { data = data });
            }
            catch (Exception)
            {
                return Json(new { data = new List<object>() });
            }
        }

        #endregion

        #region Inventory Reports

        // GET: Reports/LowStock
        public IActionResult LowStock()
        {
            return View();
        }

        // GET: Reports/GetLowStockData
        public async Task<IActionResult> GetLowStockData(int? threshold)
        {
            try
            {
                // Set default threshold value if not provided
                var stockThreshold = threshold ?? 10; // Default value is 10

                // Query inventory items with stock less than or equal to the threshold value
                var data = await _context.InventoryItems
                    .Where(i => i.Quantity <= stockThreshold) // Using the threshold value
                    .OrderBy(i => i.Quantity)
                    .Select(i => new
                    {
                        Id = i.Id,
                        ItemName = i.Name,
                        Category = i.Category ?? "General",
                        CurrentStock = i.Quantity,
                        MinimumStock = stockThreshold, // Use the threshold as the minimum stock
                        LastRestocked = i.LastRestockDate.HasValue ? i.LastRestockDate.Value.ToString("yyyy-MM-dd") : "Never",
                        Supplier = i.SupplierName ?? "N/A"
                    })
                    .ToListAsync();

                var summary = new
                {
                    TotalLowStockItems = data.Count,
                    CriticalItems = data.Count(d => d.CurrentStock <= 5),
                    OutOfStockItems = data.Count(d => d.CurrentStock == 0)
                };

                return Json(new { data = data, summary = summary });
            }
            catch (Exception)
            {
                return Json(new { data = new List<object>(), summary = new { } });
            }
        }

        #endregion

        #region HR Reports

        // GET: Reports/Payslip
        public IActionResult Payslip()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportPayslipCsv(string payPeriod, int? employeeId, string department)
        {
            // Parse period
            int year = DateTime.Today.Year, month = DateTime.Today.Month;
            if (!string.IsNullOrEmpty(payPeriod) &&
                DateTime.TryParseExact(payPeriod + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsed))
            {
                year = parsed.Year;
                month = parsed.Month;
            }

            // Find the payroll run for this period
            var run = await _context.PayrollRuns.FirstOrDefaultAsync(r => r.Year == year && r.Month == month);
            if (run == null)
                return NotFound("No payroll run found for the requested period.");

            // Build query for payroll items
            var query = _context.PayrollItems
                .Include(x => x.Employee)
                .Where(x => x.PayrollRunId == run.Id);

            if (employeeId.HasValue)
                query = query.Where(x => x.EmployeeId == employeeId);
            if (!string.IsNullOrEmpty(department))
                query = query.Where(x => x.Employee.Department.DepartmentName == department);

            var data = await query.ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Employee Name,Position,Department,Pay Date,Status,Basic Salary,Overtime,Bonuses,Allowances,Gross Pay,Tax,Social Security,Health Insurance,Other Deductions,Total Deductions,Net Pay,Payment Method,Account Number");

            foreach (var item in data)
            {
                // Replace hardcoded values as appropriate if you store or calculate them in future.
                var employee = item.Employee;
                string payDate = run.RunAt.ToString("yyyy-MM-dd");
                string status = "Paid";
                decimal overtime = 0m;
                decimal bonuses = 0m;
                decimal grossPay = item.BaseSalary + item.Allowances; // Adjust if you store GrossPay
                decimal tax = 0m;
                decimal socialSecurity = 0m;
                decimal healthInsurance = 0m;
                decimal otherDeductions = 0m;
                string paymentMethod = "Direct Deposit";
                string accountNumber = "****1234";

                sb.AppendLine($"\"{employee.FullName}\",\"{employee.Designation}\",\"{employee.Department}\",\"{payDate}\",\"{status}\",{item.BaseSalary:N2},{overtime:N2},{bonuses:N2},{item.Allowances:N2},{grossPay:N2},{tax:N2},{socialSecurity:N2},{healthInsurance:N2},{otherDeductions:N2},{item.Deductions:N2},{item.NetPay:N2},\"{paymentMethod}\",\"{accountNumber}\"");
            }

            string filename = $"Payslips_{year}_{month:00}.csv";
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", filename);
        }


        // GET: Reports/MonthlyAttendanceSheet
        public IActionResult MonthlyAttendanceSheet()
        {
            return View();
        }

        // GET: Reports/PerformanceAppraisalHistory
        public IActionResult PerformanceAppraisalHistory()
        {
            return View();
        }

        // GET: Reports/StaffCostSummary
        public IActionResult StaffCostSummary()
        {
            return View();
        }

        #endregion

        #region Export Methods

        // Export methods for each report
        [HttpGet]
        public async Task<IActionResult> ExportDailyCollectionCsv(DateTime? date, string paymentMethod)
        {
            var data = await GetDailyCollectionDataForExport(date, paymentMethod);

            if (data == null || data.Count == 0)
                return Content("No data available for export");

            var csv = new StringBuilder();
            csv.AppendLine("Invoice Number,Patient Name,Doctor Name,Amount,Payment Method,Payment Date,Notes");
            foreach (var item in data)
            {
                csv.AppendLine(
                    $"{EscapeCsvField(item.InvoiceNumber)}," +
                    $"{EscapeCsvField(item.PatientName)}," +
                    $"{EscapeCsvField(item.DoctorName)}," +
                    $"{item.Amount}," +
                    $"{EscapeCsvField(item.PaymentMethod)}," +
                    $"{EscapeCsvField(item.PaymentDate)}," +
                    $"{EscapeCsvField(item.Notes)}"
                );
            }
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"DailyCollection_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }




        public async Task<IActionResult> ExportOutstandingInvoicesCsv(string statusFilter)
        {
            var data = await GetOutstandingInvoicesDataForExport(statusFilter);
            return GenerateCsv(data, "OutstandingInvoices");
        }

        public async Task<IActionResult> ExportDoctorRevenueShareCsv(DateTime? startDate, DateTime? endDate, int? doctorId)
        {
            var data = await GetDoctorRevenueShareDataForExport(startDate, endDate, doctorId);
            return GenerateCsv(data, "DoctorRevenueShare");
        }

        public async Task<IActionResult> ExportTreatmentStatisticsCsv(DateTime? startDate, DateTime? endDate)
        {
            var data = await GetTreatmentStatisticsDataForExport(startDate, endDate);
            return GenerateCsv(data, "TreatmentStatistics");
        }

        public async Task<IActionResult> ExportLowStockCsv(int? threshold)
        {
            var data = await GetLowStockDataForExport(threshold);
            return GenerateCsv(data, "LowStock");
        }
 
        [HttpGet]
        public async Task<IActionResult> AppointListExport(
    string startDate, string endDate, int? doctorId, string status)
        {
            try
            {
                // Parse dates (if provided), else null means 'all'
                DateTime? start = null, end = null;
                var format = "yyyy-MM-dd";
                var culture = System.Globalization.CultureInfo.InvariantCulture;

                if (!string.IsNullOrWhiteSpace(startDate) &&
                    DateTime.TryParseExact(startDate, format, culture, System.Globalization.DateTimeStyles.None, out DateTime s))
                    start = s;

                if (!string.IsNullOrWhiteSpace(endDate) &&
                    DateTime.TryParseExact(endDate, format, culture, System.Globalization.DateTimeStyles.None, out DateTime e))
                    end = e;

                DateTime? endInclusive = end.HasValue ? end.Value.AddDays(1).AddTicks(-1) : (DateTime?)null;

                // Use your existing appointment data method
                var data = await GetDoctorAppointmentListData(start, endInclusive, doctorId, status);

                if (data == null || data.Count == 0)
                    return Content("No data available for export");

                var csv = new StringBuilder();
                csv.AppendLine("Appointment Date,Patient,Doctor,Treatment,Time,Duration,Status,Notes");
                foreach (var item in data)
                {
                    csv.AppendLine($"{item.AppointmentDate:yyyy-MM-dd},{EscapeCsvField(item.PatientName)}," +
                        $"{EscapeCsvField(item.DoctorName)},{EscapeCsvField(item.Treatment)}," +
                        $"{item.Time},{item.Duration},{EscapeCsvField(item.Status)},{EscapeCsvField(item.Notes)}");
                }
                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"Appointments_{DateTime.Now:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                return BadRequest("Export failed: " + ex.Message);
            }
        }

        // CSV field escaping helper

        // Helper for escaping CSV



        #endregion

        #region Private Helper Methods

        private async Task<List<dynamic>> GetDailyCollectionDataForExport(DateTime? date, string paymentMethod)
        {
            var query = _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Patient)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Doctor)
                .Include(p => p.PaymentMethod)
                .Where(p => p.PaymentDate != null && p.Status == PaymentStatus.Completed);

            if (date.HasValue)
            {
                query = query.Where(p =>
                    EF.Functions.DateDiffDay(p.PaymentDate.Value.Date, date.Value.Date) == 0
                );
            }

            if (!string.IsNullOrEmpty(paymentMethod))
            {
                query = query.Where(p => p.PaymentMethod.Name == paymentMethod);
            }

            var result = await query
                .OrderBy(p => p.PaymentDate)
                .Select(p => new
                {
                    InvoiceNumber = p.Invoice.InvoiceNumber,
                    PatientName = p.Invoice.Patient.FirstName + " " + p.Invoice.Patient.LastName,
                    DoctorName = p.Invoice.Doctor.FullName,
                    Amount = p.Total,
                    PaymentMethod = p.PaymentMethod.Name,
                    PaymentDate = p.PaymentDate.Value.ToString("yyyy-MM-dd HH:mm"),
                    Notes = p.Notes ?? ""
                })
                .ToListAsync();

            return result.Cast<dynamic>().ToList();
        }

        private async Task<List<object>> GetOutstandingInvoicesDataForExport(string statusFilter)
        {
            var query = _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Doctor)
                .Where(i => i.PaymentStatus != PaymentStatus.Paid && i.DueDate <= DateTime.Now);

            if (!string.IsNullOrEmpty(statusFilter))
            {
                if (statusFilter == "Overdue")
                {
                    query = query.Where(i => i.DueDate < DateTime.Today);
                }
                else if (statusFilter == "DueToday")
                {
                    query = query.Where(i => i.DueDate.Date == DateTime.Today);
                }
            }

            var result = await query
                .OrderBy(i => i.DueDate)
                .Select(i => new
                {
                    InvoiceNumber = i.InvoiceNumber,
                    PatientName = i.Patient.FirstName + " " + i.Patient.LastName,
                    DoctorName = i.Doctor.FullName,
                    IssueDate = i.IssueDate.ToString("yyyy-MM-dd"),
                    DueDate = i.DueDate.ToString("yyyy-MM-dd"),
                    TotalAmount = CalculateInvoiceTotal(i),
                    AmountPaid = i.AmountPaid,
                    BalanceDue = CalculateInvoiceTotal(i) - i.AmountPaid,
                    Status = i.PaymentStatus == PaymentStatus.Paid ? "Paid" : i.DueDate < DateTime.Today ? "Overdue" : "Pending",
                    OverdueDays = (DateTime.Now - i.DueDate).Days
                })
                .ToListAsync();

            return result.Cast<object>().ToList();
        }

        private async Task<List<object>> GetDoctorRevenueShareDataForExport(DateTime? startDate, DateTime? endDate, int? doctorId)
        {
            var query = _context.Invoices
                .Include(i => i.Doctor)
                .Include(i => i.Patient)
                .Where(i => i.PaymentStatus == PaymentStatus.Paid);

            if (startDate.HasValue)
                query = query.Where(i => i.IssueDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(i => i.IssueDate <= endDate.Value);
            if (doctorId.HasValue)
                query = query.Where(i => i.DoctorId == doctorId.Value);

            var result = await query
                .GroupBy(i => new { i.DoctorId, i.Doctor.FullName })
                .Select(g => new
                {
                    DoctorId = g.Key.DoctorId,
                    DoctorName = g.Key.FullName,
                    TotalRevenue = g.Sum(i => i.Total),
                    RevenueShare = g.Sum(i => i.Total) * 0.7m,
                    AppointmentCount = g.Count(),
                    AverageRevenuePerAppointment = g.Average(i => i.Total)
                })
                .OrderByDescending(d => d.TotalRevenue)
                .ToListAsync();

            return result.Cast<object>().ToList();
        }

        private async Task<List<object>> GetTreatmentStatisticsDataForExport(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Appointments
                .Where(a => a.Status == "Completed");

            if (startDate.HasValue)
                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Appointment, Doctor>)query.Where(a => a.AppointmentDate >= startDate.Value);
            if (endDate.HasValue)
                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Appointment, Doctor>)query.Where(a => a.AppointmentDate <= endDate.Value);

            var result = await query
                .GroupBy(a => new { TreatmentType = a.Treatments.Name ?? "General Checkup" })
                .Select(g => new
                {
                    TreatmentName = g.Key.TreatmentType,
                    Category = "Dental",
                    AppointmentCount = g.Count(),
                    TotalRevenue = g.Sum(a => a.Fee),
                    AverageDuration = g.Average(a => a.Fee)
                })
                .OrderByDescending(t => t.AppointmentCount)
                .ToListAsync();

            return result.Cast<object>().ToList();
        }

        private async Task<List<object>> GetLowStockDataForExport(int? threshold)
        {
            var stockThreshold = threshold ?? 10;

            var result = await _context.InventoryItems
                .Where(i => i.Quantity <= stockThreshold)
                .OrderBy(i => i.Quantity)
                .Select(i => new
                {
                    Id = i.Id,
                    ItemName = i.Name,
                    Category = i.Category ?? "General",
                    CurrentStock = i.Quantity,
                    MinimumStock =  5,
                    Unit = i.Quantity,
                    LastRestocked = i.LastRestockDate.HasValue ?
                        i.LastRestockDate.Value.ToString("yyyy-MM-dd") : "Never",
                    Supplier = i.SupplierName ?? "N/A"
                })
                .ToListAsync();

            return result.Cast<object>().ToList();
        }

        private IActionResult GenerateCsv(List<object> data, string reportName)
        {
            if (data == null || !data.Any())
                return Content("No data available for export");

            var sb = new StringBuilder();

            // Add headers based on the first object's properties
            if (data.Any())
            {
                var firstItem = data.First();
                var properties = firstItem.GetType().GetProperties();
                sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));
            }

            // Add data rows
            foreach (var item in data)
            {
                var properties = item.GetType().GetProperties();
                var values = properties.Select(p =>
                {
                    var value = p.GetValue(item)?.ToString() ?? "";
                    return $"\"{value.Replace("\"", "\"\"")}\"";
                });
                sb.AppendLine(string.Join(",", values));
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv",
                $"{reportName}_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        #endregion


         [HttpGet]
    public async Task<IActionResult> GetDoctors()
    {
        var doctors = await _context.Doctors
            .Select(d => new { id = d.Id, text = d.FullName })
            .OrderBy(d => d.text)
            .ToListAsync();

        return Json(new { success = true, data = doctors });
    }

        private IActionResult ExportToCSV(List<AppointmentExportVM> data)
        {
            var csv = new StringBuilder();
            csv.AppendLine("Appointment Date,Patient,Doctor,Treatment,Time,Duration,Status,Notes");

            foreach (var item in data)
            {
                string timeValue;
                if (item.Time is TimeSpan timeSpan)
                {
                    timeValue = timeSpan.ToString(@"hh\:mm");
                }
                else if (item.Time != null)
                {
                    // Try parse string as TimeSpan, fallback to plain string
                    if (TimeSpan.TryParse(item.Time.ToString(), out TimeSpan parsedSpan))
                        timeValue = parsedSpan.ToString(@"hh\:mm");
                    else
                        timeValue = item.Time.ToString();
                }
                else
                {
                    timeValue = "-";
                }

                csv.AppendLine($"{item.AppointmentDate:yyyy-MM-dd}," +
                               $"\"{EscapeCsvField(item.PatientName)}\"," +
                               $"\"{EscapeCsvField(item.DoctorName)}\"," +
                               $"\"{EscapeCsvField(item.Treatment)}\"," +
                               $"\"{timeValue}\"," +
                               $"{item.Duration}," +
                               $"\"{EscapeCsvField(item.Status)}\"," +
                               $"\"{EscapeCsvField(item.Notes)}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"DoctorAppointments_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return "\"\"";

            // If field contains comma, quote, or newline, wrap in quotes and escape existing quotes
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\r") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }

            return field;
        }



        // Dynamic attendance data (AJAX endpoint)
        [HttpGet]
        public async Task<IActionResult> GetMonthlyAttendanceData(string month, string department, int? employeeId)
        {
            // Parse month (YYYY-MM) into year/month
            int year = DateTime.Today.Year;
            int mon = DateTime.Today.Month;
            if (!string.IsNullOrEmpty(month) && DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsed))
            {
                year = parsed.Year;
                mon = parsed.Month;
            }

            var query = _context.AttendanceRecords
                .Include(r => r.Employee)
                .Include(r => r.Employee.Department)
                .Where(r => r.Date.Year == year && r.Date.Month == mon);

            if (!string.IsNullOrEmpty(department))
                query = query.Where(r => r.Employee.Department.DepartmentName == department);

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            var grouped = query.GroupBy(r => r.Employee);

            var data = await grouped.Select(g => new
            {
                employeeName = g.Key.FullName,
                department = g.Key.Department.DepartmentName,
                workingDays = g.Count(),
                present = g.Count(x => x.Status == AttendanceStatus.Present),
                absent = g.Count(x => x.Status == AttendanceStatus.Absent),
                lateArrivals = g.Count(x => x.Status == AttendanceStatus.Late),
                earlyDepartures = g.Count(x => x.Status == AttendanceStatus.HalfDay),
                attendancePercentage = Math.Round((double)g.Count(x => x.Status == AttendanceStatus.Present) / g.Count() * 100, 1)
            }).ToListAsync();

            int totalStaff = data.Count;
            int totalAbsences = data.Sum(x => x.absent);
            int lateArrivals = data.Sum(x => x.lateArrivals);
            double avgAttendance = totalStaff > 0 ? Math.Round(data.Average(x => x.attendancePercentage), 1) : 0;

            var summary = new
            {
                totalStaff,
                averageAttendance = avgAttendance,
                totalAbsences,
                lateArrivals
            };

            return Json(new { data, summary });
        }


        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            // Assuming Doctors and Staff have Id and FullName properties
            var doctors = await _context.Doctors
                .Where(d => d.AvailabilityStatus== "Availble")
                .Select(d => new {
                    id = d.Id,
                    text = d.FullName
                }).ToListAsync();

            var staff = await _context.Employees
                .Where(s => s.IsActive)
                .Select(s => new {
                    id = s.Id + 10000, // Offset Staff or add prefix to distinguish if needed
                    text = s.FullName
                }).ToListAsync();

            var employees = doctors.Concat(staff).OrderBy(e => e.text).ToList();

            // Add "All Employees" on top
            employees.Insert(0, new { id = 0, text = "All Employees" });

            return Json(new { results = employees });
        }


        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .OrderBy(d => d.DepartmentName)
                .Select(d => new
                {
                    id = d.DepartmentName, // Or d.Id if you prefer IDs
                    text = d.DepartmentName
                })
                .ToListAsync();

            // Optionally, insert "All Departments" at the top
            // Use id = 0 for "All Departments" to match the int type
            departments.Insert(0, new { id = "", text = "All Departments" });

            return Json(new { results = departments });
        }


        [HttpGet]
        public async Task<IActionResult> GetPayslipData(string payPeriod, int? employeeId, string department)
        {
            // Parse period
            int year = DateTime.Today.Year, month = DateTime.Today.Month;
            if (!string.IsNullOrEmpty(payPeriod) &&
                DateTime.TryParseExact(payPeriod + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsed))
            {
                year = parsed.Year;
                month = parsed.Month;
            }

            // Find matching PayrollRun
            var run = await _context.PayrollRuns.FirstOrDefaultAsync(r => r.Year == year && r.Month == month);
            if (run == null)
                return Json(new List<object>());

            // Payroll items for this run
            var query = _context.PayrollItems
                .Include(x => x.Employee)
                .ThenInclude(e => e.Department)
                .Where(x => x.PayrollRunId == run.Id);

            if (employeeId.HasValue) query = query.Where(x => x.EmployeeId == employeeId);
            if (!string.IsNullOrEmpty(department)) query = query.Where(x => x.Employee.Department.DepartmentName == department);

            var data = await query.Select(x => new {
                employeeName = x.Employee.FullName,
                position = x.Employee.Designation,
                department = x.Employee.Department,
                payDate = run.RunAt.ToString("yyyy-MM-dd"),
                status = "Paid",
                basicSalary = x.BaseSalary,
                overtime = 0m, // Add if stored, else 0
                bonuses = 0m,  // Add if stored, else 0
                allowances = x.Allowances,
                grossPay = x.BaseSalary + x.Allowances,
                tax = 0m,               // Add if stored, else 0 (or expose real field)
                socialSecurity = 0m,    // Add if stored, else 0 (or expose real field)
                healthInsurance = 0m,   // Add if stored, else 0
                otherDeductions = 0m,   // Add if stored, else 0
                totalDeductions = x.Deductions,
                netPay = x.NetPay,
                paymentMethod = x.Employee.PaymentMethodId ?? 0 , // Use real field if available
                accountNumber = x.Employee.AccountNumber ?? "Account Number not found" // Use real field if available
            }).ToListAsync();

            return Json(data);
        }


        // CSV Export
        [HttpGet]
        public async Task<IActionResult> ExportMonthlyAttendanceCsv(string month, string department, int? employeeId)
        {
            // Use same data logic
            int year = DateTime.Today.Year;
            int mon = DateTime.Today.Month;
            if (!string.IsNullOrEmpty(month) && DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out DateTime parsed))
            {
                year = parsed.Year;
                mon = parsed.Month;
            }

            var query = _context.AttendanceRecords
                .Include(r => r.Employee)
                .Include(r => r.Employee.Department)
                .Where(r => r.Date.Year == year && r.Date.Month == mon);

            if (!string.IsNullOrEmpty(department))
                query = query.Where(r => r.Employee.Department.DepartmentName == department);

            if (employeeId.HasValue)
                query = query.Where(r => r.EmployeeId == employeeId.Value);

            var grouped = query.GroupBy(r => r.Employee);

            var data = await grouped.Select(g => new
            {
                Employee = g.Key.FullName,
                Department = g.Key.Department.DepartmentName,
                WorkingDays = g.Count(),
                Present = g.Count(x => x.Status == AttendanceStatus.Present),
                Absent = g.Count(x => x.Status == AttendanceStatus.Absent),
                LateArrivals = g.Count(x => x.Status == AttendanceStatus.Late),
                EarlyDepartures = g.Count(x => x.Status == AttendanceStatus.HalfDay),
                AttendancePercent = Math.Round((double)g.Count(x => x.Status == AttendanceStatus.Present) / g.Count() * 100, 1),
                Status = ((double)g.Count(x => x.Status == AttendanceStatus.Present) / g.Count() * 100) >= 95 ? "Excellent" :
                         ((double)g.Count(x => x.Status == AttendanceStatus.Present) / g.Count() * 100) >= 90 ? "Good" : "Needs Improvement"
            }).ToListAsync();

            if (!data.Any())
                return Content("No data available for export");

            var csv = new StringBuilder();
            csv.AppendLine("Employee,Department,Working Days,Present,Absent,Late Arrivals,Early Departures,Attendance %,Status");
            foreach (var x in data)
            {
                csv.AppendLine($"{EscapeCsv(x.Employee)},{EscapeCsv(x.Department)},{x.WorkingDays},{x.Present},{x.Absent},{x.LateArrivals},{x.EarlyDepartures},{x.AttendancePercent},{EscapeCsv(x.Status)}");
            }
            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"MonthlyAttendance_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }

        private string EscapeCsv(string val)
        {
            if (string.IsNullOrEmpty(val)) return "";
            if (val.Contains(",") || val.Contains("\"") || val.Contains("\n"))
                return $"\"{val.Replace("\"", "\"\"")}\"";
            return val;
        }


    }
}