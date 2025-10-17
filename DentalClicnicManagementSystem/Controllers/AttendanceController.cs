using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Controllers
{
    //[Authorize(Roles = "HR,Admin")]
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Attendance
        public IActionResult Index()
        {
            return View();
        }

        // GET: Attendance/GetActiveEmployees
        public async Task<IActionResult> GetActiveEmployees()
        {
            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new {
                    id = e.Id,
                    fullName = e.FirstName + " " + e.LastName,
                    code = e.Code
                })
                .ToListAsync();
            return Json(employees);
        }

        // GET: Attendance/GetAttendanceRecords

        public async Task<IActionResult> GetAttendanceRecords()
        {
            try
            {
                // Check if using new clean parameters or old DataTables parameters
                var hasNewParams = HttpContext.Request.Query.ContainsKey("orderColumn");

                if (hasNewParams)
                {
                    // NEW PARAMETERS (from your clean JavaScript)
                    var draw = HttpContext.Request.Query["draw"].FirstOrDefault() ?? "1";
                    var start = int.Parse(HttpContext.Request.Query["start"].FirstOrDefault() ?? "0");
                    var length = int.Parse(HttpContext.Request.Query["length"].FirstOrDefault() ?? "10");
                    var searchValue = HttpContext.Request.Query["search"].FirstOrDefault() ?? "";
                    var orderColumn = int.Parse(HttpContext.Request.Query["orderColumn"].FirstOrDefault() ?? "2");
                    var orderDirection = HttpContext.Request.Query["orderDirection"].FirstOrDefault() ?? "desc";
                    var statusFilter = HttpContext.Request.Query["status"].FirstOrDefault() ?? "all";

                    return await ProcessAttendanceRequest(draw, start, length, searchValue, orderColumn, orderDirection, statusFilter);
                }
                else
                {
                    // OLD PARAMETERS (fallback for compatibility)
                    var draw = HttpContext.Request.Query["draw"].ToString();
                    var start = int.Parse(HttpContext.Request.Query["start"].ToString());
                    var length = int.Parse(HttpContext.Request.Query["length"].ToString());
                    var searchValue = HttpContext.Request.Query["search[value]"].ToString();
                    var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
                    var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();
                    var statusFilter = HttpContext.Request.Query["status"].ToString();

                    return await ProcessAttendanceRequest(draw, start, length, searchValue, sortColumnIndex, sortColumnDir, statusFilter);
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                // _logger.LogError(ex, "Error in GetAttendanceRecords");

                return Json(new
                {
                    draw = "1",
                    recordsFiltered = 0,
                    recordsTotal = 0,
                    data = new List<object>(),
                    error = ex.Message // Remove in production
                });
            }
        }

        private async Task<IActionResult> ProcessAttendanceRequest(string draw, int start, int length, string searchValue, int sortColumnIndex, string sortColumnDir, string statusFilter)
        {
            var records = _context.AttendanceRecords
                                  .Include(ar => ar.Employee)
                                  .AsQueryable();

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                if (Enum.TryParse<AttendanceStatus>(statusFilter, true, out var status))
                {
                    records = records.Where(ar => ar.Status == status);
                }
            }

            // Filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                records = records.Where(ar =>
                    ar.Employee.FirstName.Contains(searchValue) ||
                    ar.Employee.LastName.Contains(searchValue) ||
                    ar.Employee.Code.Contains(searchValue) ||
                    ar.Note.Contains(searchValue) ||
                    ar.Status.ToString().Contains(searchValue));
            }

            // Map the new column indexes to your data structure
            var columnMapping = new Dictionary<int, string>
    {
        { 0, "employeeCode" },
        { 1, "employeeName" },
        { 2, "date" },
        { 3, "checkIn" },
        { 4, "checkOut" },
        { 6, "status" },
        { 7, "note" }
    };

            var sortColumn = columnMapping.ContainsKey(sortColumnIndex) ? columnMapping[sortColumnIndex] : "date";

            // Sorting logic (same as before but using the mapped column)
            if (!string.IsNullOrEmpty(sortColumn))
            {
                if (sortColumn == "employeeName")
                {
                    records = sortColumnDir == "asc" ?
                        records.OrderBy(ar => ar.Employee.FirstName).ThenBy(ar => ar.Employee.LastName) :
                        records.OrderByDescending(ar => ar.Employee.FirstName).ThenByDescending(ar => ar.Employee.LastName);
                }
                else if (sortColumn == "employeeCode")
                {
                    records = sortColumnDir == "asc" ?
                        records.OrderBy(ar => ar.Employee.Code) :
                        records.OrderByDescending(ar => ar.Employee.Code);
                }
                else
                {
                    var propertyName = sortColumn switch
                    {
                        "date" => "Date",
                        "status" => "Status",
                        "checkIn" => "CheckIn",
                        "checkOut" => "CheckOut",
                        "note" => "Note",
                        _ => "Date" // Default to Date
                    };

                    records = sortColumnDir == "asc" ?
                        records.OrderBy(ar => EF.Property<object>(ar, propertyName)) :
                        records.OrderByDescending(ar => EF.Property<object>(ar, propertyName));
                }
            }
            else
            {
                records = records.OrderByDescending(ar => ar.Date);
            }

            var recordsTotal = await records.CountAsync();

            var data = await records.Skip(start).Take(length).Select(ar => new
            {
                ar.Id,
                EmployeeId = ar.Employee.Id,
                EmployeeName = ar.Employee.FirstName + " " + ar.Employee.LastName,
                EmployeeCode = ar.Employee.Code,
                Date = ar.Date.ToString("yyyy-MM-dd"),
                CheckIn = ar.CheckIn.HasValue ? ar.CheckIn.Value.ToString("HH:mm") : "",
                CheckOut = ar.CheckOut.HasValue ? ar.CheckOut.Value.ToString("HH:mm") : "",
                Status = ar.Status.ToString(),
                ar.Note
            }).ToListAsync();

            return Json(new
            {
                draw = draw,
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                data = data
            });
        }
        //public async Task<IActionResult> GetAttendanceRecords()
        //{
        //    try
        //    {
        //        var draw = HttpContext.Request.Query["draw"].ToString();
        //        var start = int.Parse(HttpContext.Request.Query["start"].ToString());
        //        var length = int.Parse(HttpContext.Request.Query["length"].ToString());
        //        var searchValue = HttpContext.Request.Query["search[value]"].ToString();
        //        var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
        //        var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();
        //        var statusFilter = HttpContext.Request.Query["status"].ToString();

        //        var records = _context.AttendanceRecords
        //                              .Include(ar => ar.Employee)
        //                              .AsQueryable();

        //        // Apply status filter
        //        if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
        //        {
        //            if (Enum.TryParse<AttendanceStatus>(statusFilter, true, out var status))
        //            {
        //                records = records.Where(ar => ar.Status == status);
        //            }
        //        }

        //        // Filtering
        //        if (!string.IsNullOrEmpty(searchValue))
        //        {
        //            records = records.Where(ar =>
        //                ar.Employee.FirstName.Contains(searchValue) ||
        //                ar.Employee.LastName.Contains(searchValue) ||
        //                ar.Employee.Code.Contains(searchValue) ||
        //                ar.Note.Contains(searchValue) ||
        //                ar.Status.ToString().Contains(searchValue));
        //        }

        //        // Sorting
        //        var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
        //        if (!string.IsNullOrEmpty(sortColumn))
        //        {
        //            if (sortColumn == "employeeName")
        //            {
        //                records = sortColumnDir == "asc" ?
        //                    records.OrderBy(ar => ar.Employee.FirstName).ThenBy(ar => ar.Employee.LastName) :
        //                    records.OrderByDescending(ar => ar.Employee.FirstName).ThenByDescending(ar => ar.Employee.LastName);
        //            }
        //            else if (sortColumn == "employeeCode")
        //            {
        //                records = sortColumnDir == "asc" ?
        //                    records.OrderBy(ar => ar.Employee.Code) :
        //                    records.OrderByDescending(ar => ar.Employee.Code);
        //            }
        //            else
        //            {
        //                var propertyName = sortColumn switch
        //                {
        //                    "date" => "Date",
        //                    "status" => "Status",
        //                    "checkIn" => "CheckIn",
        //                    "checkOut" => "CheckOut",
        //                    "note" => "Note",
        //                    _ => null
        //                };

        //                if (propertyName != null)
        //                {
        //                    records = sortColumnDir == "asc" ?
        //                        records.OrderBy(ar => EF.Property<object>(ar, propertyName)) :
        //                        records.OrderByDescending(ar => EF.Property<object>(ar, propertyName));
        //                }
        //                else
        //                {
        //                    records = records.OrderByDescending(ar => ar.Date);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            records = records.OrderByDescending(ar => ar.Date);
        //        }

        //        var recordsTotal = await records.CountAsync();
        //        var data = await records.Skip(start).Take(length).Select(ar => new
        //        {
        //            ar.Id,
        //            EmployeeId = ar.Employee.Id,
        //            EmployeeName = ar.Employee.FirstName + " " + ar.Employee.LastName,
        //            EmployeeCode = ar.Employee.Code,
        //            Date = ar.Date.ToString("yyyy-MM-dd"),
        //            CheckIn = ar.CheckIn.HasValue ? ar.CheckIn.Value.ToString("HH:mm") : "",
        //            CheckOut = ar.CheckOut.HasValue ? ar.CheckOut.Value.ToString("HH:mm") : "",
        //            Status = ar.Status.ToString(),
        //            ar.Note
        //        }).ToListAsync();

        //        return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { draw = "0", recordsFiltered = 0, recordsTotal = 0, data = new List<object>() });
        //    }
        //}

        // GET: Attendance/Upsert
        public async Task<IActionResult> Upsert(int? id)
        {
            AttendanceRecord record = new AttendanceRecord();
            if (id != null)
            {
                record = await _context.AttendanceRecords.FindAsync(id);
                if (record == null)
                {
                    return NotFound();
                }
            }

            ViewData["EmployeeList"] = new SelectList(await _context.Employees.Where(e => e.IsActive).ToListAsync(), "Id", "FullName", record.EmployeeId);
            return PartialView("_UpsertPartial", record);
        }

        // POST: Attendance/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(AttendanceRecord record)
        {
            if (ModelState.IsValid)
            {
                if (record.Id == 0)
                {
                    var existingRecord = await _context.AttendanceRecords
                                                       .FirstOrDefaultAsync(ar => ar.EmployeeId == record.EmployeeId && ar.Date.Date == record.Date.Date);
                    if (existingRecord != null)
                    {
                        return Json(new { status = false, message = "An attendance record for this employee on this date already exists. Please edit the existing record." });
                    }

                    _context.AttendanceRecords.Add(record);
                }
                else
                {
                    var existingRecord = await _context.AttendanceRecords
                                                       .FirstOrDefaultAsync(ar => ar.EmployeeId == record.EmployeeId && ar.Date.Date == record.Date.Date && ar.Id != record.Id);
                    if (existingRecord != null)
                    {
                        return Json(new { status = false, message = "An attendance record for this employee on this date already exists. Please edit the existing record." });
                    }

                    _context.AttendanceRecords.Update(record);
                }
                await _context.SaveChangesAsync();
                TempData["success"] = "Attendance record saved successfully!";
                return RedirectToAction(nameof(Index));

                //return Json(new { status = true, message = "Attendance record saved successfully!" });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            TempData["error"] = "Fail to record Validation error!";
            return RedirectToAction(nameof(Index));
            //return Json(new { status = false, message = "Validation failed: " + string.Join("; ", errors) });
        }

        // POST: Attendance/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.AttendanceRecords.FindAsync(id);
            if (record == null)
            {
                return Json(new { status = false, message = "Attendance record not found." });
            }

            _context.AttendanceRecords.Remove(record);
            await _context.SaveChangesAsync();

            return Json(new { status = true, message = "Attendance record deleted successfully." });
        }

        // GET: Attendance/PreviewReport
        public async Task<IActionResult> PreviewReport(string reportType, string employeeId, string week = null, string month = null, int year = 0, string startDate = null, string endDate = null)
        {
            var (start, end) = GetDateRange(reportType, week, month, year, startDate, endDate);
            if (start == DateTime.MinValue)
            {
                return BadRequest("Invalid date range");
            }

            var records = await GetAttendanceRecordsForReport(employeeId, start, end);
            var reportData = GenerateReportData(records);

            return Json(reportData);
        }

        // GET: Attendance/GenerateReport
        public async Task<IActionResult> GenerateReport(string reportType, string employeeId, string week = null, string month = null, int year = 0, string startDate = null, string endDate = null)
        {
            var (start, end) = GetDateRange(reportType, week, month, year, startDate, endDate);
            if (start == DateTime.MinValue)
            {
                return BadRequest("Invalid date range");
            }

            var records = await GetAttendanceRecordsForReport(employeeId, start, end);
            var reportData = GenerateReportData(records);

            var csv = GenerateCsvReport(reportData, start, end, reportType);
            var fileName = GetReportFileName(reportType, start, end);

            return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }

        private (DateTime start, DateTime end) GetDateRange(string reportType, string week, string month, int year, string startDate, string endDate)
        {
            switch (reportType)
            {
                case "weekly":
                    if (DateTime.TryParse(week + "-1", out var weekStart))
                    {
                        return (weekStart, weekStart.AddDays(6));
                    }
                    break;
                case "monthly":
                    if (DateTime.TryParse(month + "-01", out var monthStart))
                    {
                        return (monthStart, monthStart.AddMonths(1).AddDays(-1));
                    }
                    break;
                case "yearly":
                    return (new DateTime(year, 1, 1), new DateTime(year, 12, 31));
                case "custom":
                    if (DateTime.TryParse(startDate, out var customStart) && DateTime.TryParse(endDate, out var customEnd))
                    {
                        return (customStart, customEnd);
                    }
                    break;
            }
            return (DateTime.MinValue, DateTime.MinValue);
        }

        private async Task<List<AttendanceRecord>> GetAttendanceRecordsForReport(string employeeId, DateTime start, DateTime end)
        {
            var query = _context.AttendanceRecords
                .Include(ar => ar.Employee)
                .Where(ar => ar.Date >= start && ar.Date <= end);

            if (!string.IsNullOrEmpty(employeeId) && int.TryParse(employeeId, out int empId))
            {
                query = query.Where(ar => ar.EmployeeId == empId);
            }

            return await query.ToListAsync();
        }

        private List<object> GenerateReportData(List<AttendanceRecord> records)
        {
            var reportData = records
                .GroupBy(ar => new { ar.EmployeeId, ar.Employee.FirstName, ar.Employee.LastName, ar.Employee.Code })
                .Select(g => new
                {
                    EmployeeName = $"{g.Key.FirstName} {g.Key.LastName}",
                    EmployeeCode = g.Key.Code,
                    Present = g.Count(ar => ar.Status == AttendanceStatus.Present),
                    Late = g.Count(ar => ar.Status == AttendanceStatus.Late),
                    Absent = g.Count(ar => ar.Status == AttendanceStatus.Absent),
                    TotalHours = g.Where(ar => ar.CheckIn.HasValue && ar.CheckOut.HasValue)
                                 .Sum(ar => (ar.CheckOut.Value - ar.CheckIn.Value).TotalHours)
                })
                .ToList();

            return reportData.Cast<object>().ToList();
        }

        private string GenerateCsvReport(List<object> reportData, DateTime start, DateTime end, string reportType)
        {
            var csv = new StringBuilder();
            csv.AppendLine($"Attendance Report - {reportType.ToUpper()}");
            csv.AppendLine($"Period: {start:yyyy-MM-dd} to {end:yyyy-MM-dd}");
            csv.AppendLine();
            csv.AppendLine("Employee,Present Days,Late Days,Absent Days,Total Hours");

            foreach (dynamic item in reportData)
            {
                csv.AppendLine($"\"{item.EmployeeName}\",{item.Present},{item.Late},{item.Absent},{item.TotalHours:F2}");
            }

            // Add summary
            var totalPresent = reportData.Sum(item => (int)item.GetType().GetProperty("Present").GetValue(item));
            var totalLate = reportData.Sum(item => (int)item.GetType().GetProperty("Late").GetValue(item));
            var totalAbsent = reportData.Sum(item => (int)item.GetType().GetProperty("Absent").GetValue(item));
            var totalHours = reportData.Sum(item => (double)item.GetType().GetProperty("TotalHours").GetValue(item));

            csv.AppendLine();
            csv.AppendLine($"TOTAL,,{totalPresent},{totalLate},{totalAbsent},{totalHours:F2}");

            return csv.ToString();
        }

        private string GetReportFileName(string reportType, DateTime start, DateTime end)
        {
            return $"Attendance_Report_{reportType}_{start:yyyyMMdd}_to_{end:yyyyMMdd}.csv";
        }
    }
}