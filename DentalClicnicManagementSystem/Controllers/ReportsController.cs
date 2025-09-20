// File: Controllers/ReportsController.cs
using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Controllers
{
    //[Authorize(Roles = "HR,Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reports
        public IActionResult Index()
        {
            return View();
        }

        // GET: Reports/AttendanceReport
        public IActionResult AttendanceReport()
        {
            return View();
        }

        // GET: Reports/GetAttendanceReportData
        public async Task<IActionResult> GetAttendanceReportData(DateTime? startDate, DateTime? endDate, string statusFilter)
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            var records = _context.AttendanceRecords
                                  .Include(ar => ar.Employee)
                                  .AsQueryable();

            // Apply date range filters
            if (startDate.HasValue)
            {
                records = records.Where(ar => ar.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                records = records.Where(ar => ar.Date <= endDate.Value);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse(statusFilter, out AttendanceStatus parsedStatus))
            {
                records = records.Where(ar => ar.Status == parsedStatus);
            }

            // Apply global search
            if (!string.IsNullOrEmpty(searchValue))
            {
                records = records.Where(ar =>
                    ar.Employee.FirstName.Contains(searchValue) ||
                    ar.Employee.LastName.Contains(searchValue) ||
                    ar.Employee.Code.Contains(searchValue) ||
                    ar.Note.Contains(searchValue) ||
                    ar.Status.ToString().Contains(searchValue));
            }

            // Sorting
            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
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
                        _ => null
                    };

                    if (propertyName != null)
                    {
                        records = sortColumnDir == "asc" ?
                            records.OrderBy(ar => EF.Property<object>(ar, propertyName)) :
                            records.OrderByDescending(ar => EF.Property<object>(ar, propertyName));
                    }
                    else
                    {
                        records = records.OrderByDescending(ar => ar.Date); // Fallback
                    }
                }
            }
            else
            {
                records = records.OrderByDescending(ar => ar.Date); // Default sort by date
            }

            var recordsTotal = await records.CountAsync();
            var data = await records.Skip(start).Take(length).Select(ar => new
            {
                ar.Id,
                EmployeeId = ar.Employee.Id,
                EmployeeName = ar.Employee.FullName,
                EmployeeCode = ar.Employee.Code,
                Date = ar.Date.ToString("yyyy-MM-dd"),
                CheckIn = ar.CheckIn.HasValue ? ar.CheckIn.Value.ToString("HH:mm") : "",
                CheckOut = ar.CheckOut.HasValue ? ar.CheckOut.Value.ToString("HH:mm") : "",
                Status = ar.Status.ToString(),
                ar.Note
            }).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: Reports/ExportAttendanceCsv
        public async Task<IActionResult> ExportAttendanceCsv(DateTime? startDate, DateTime? endDate, string statusFilter)
        {
            var records = _context.AttendanceRecords
                                  .Include(ar => ar.Employee)
                                  .AsQueryable();

            if (startDate.HasValue)
            {
                records = records.Where(ar => ar.Date >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                records = records.Where(ar => ar.Date <= endDate.Value);
            }
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse(statusFilter, out AttendanceStatus parsedStatus))
            {
                records = records.Where(ar => ar.Status == parsedStatus);
            }

            var data = await records.OrderBy(ar => ar.Date).ThenBy(ar => ar.Employee.FullName).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Employee Code,Employee Name,Date,Check-in,Check-out,Status,Note");

            foreach (var item in data)
            {
                sb.AppendLine($"{item.Employee.Code},{item.Employee.FullName},{item.Date:yyyy-MM-dd},{item.CheckIn?.ToString("HH:mm")},{item.CheckOut?.ToString("HH:mm")},{item.Status},{item.Note?.Replace(",", ";")}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "AttendanceReport.csv");
        }


        // GET: Reports/LeaveReport
        public IActionResult LeaveReport()
        {
            return View();
        }

        // GET: Reports/GetLeaveReportData
        public async Task<IActionResult> GetLeaveReportData(DateTime? startDate, DateTime? endDate, string statusFilter, string typeFilter)
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            var requests = _context.LeaveRequests
                                   .Include(lr => lr.Employee)
                                   .AsQueryable();

            // Apply date range filters
            if (startDate.HasValue)
            {
                requests = requests.Where(lr => lr.StartDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                requests = requests.Where(lr => lr.EndDate <= endDate.Value);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse(statusFilter, out LeaveStatus parsedStatus))
            {
                requests = requests.Where(lr => lr.Status == parsedStatus);
            }

            // Apply type filter
            if (!string.IsNullOrEmpty(typeFilter) && Enum.TryParse(typeFilter, out LeaveTypeEnum parsedType))
            {
                requests = requests.Where(lr => lr.Type == parsedType);
            }

            // Apply global search
            if (!string.IsNullOrEmpty(searchValue))
            {
                requests = requests.Where(lr =>
                    lr.Employee.FirstName.Contains(searchValue) ||
                    lr.Employee.LastName.Contains(searchValue) ||
                    lr.Employee.Code.Contains(searchValue) ||
                    lr.Reason.Contains(searchValue) ||
                    lr.Type.ToString().Contains(searchValue) ||
                    lr.Status.ToString().Contains(searchValue));
            }

            // Sorting
            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
            if (!string.IsNullOrEmpty(sortColumn))
            {
                if (sortColumn == "employeeName")
                {
                    requests = sortColumnDir == "asc" ?
                        requests.OrderBy(lr => lr.Employee.FirstName).ThenBy(lr => lr.Employee.LastName) :
                        requests.OrderByDescending(lr => lr.Employee.FirstName).ThenByDescending(lr => lr.Employee.LastName);
                }
                else if (sortColumn == "employeeCode")
                {
                    requests = sortColumnDir == "asc" ?
                        requests.OrderBy(lr => lr.Employee.Code) :
                        requests.OrderByDescending(lr => lr.Employee.Code);
                }
                else
                {
                    var propertyName = sortColumn switch
                    {
                        "type" => "Type",
                        "startDate" => "StartDate",
                        "endDate" => "EndDate",
                        "days" => "Days",
                        "reason" => "Reason",
                        "status" => "Status",
                        "decisionBy" => "DecisionBy",
                        "decisionAt" => "DecisionAt",
                        _ => null
                    };

                    if (propertyName != null)
                    {
                        requests = sortColumnDir == "asc" ?
                            requests.OrderBy(lr => EF.Property<object>(lr, propertyName)) :
                            requests.OrderByDescending(lr => EF.Property<object>(lr, propertyName));
                    }
                    else
                    {
                        requests = requests.OrderByDescending(lr => lr.StartDate); // Fallback
                    }
                }
            }
            else
            {
                requests = requests.OrderByDescending(lr => lr.StartDate); // Default sort by start date
            }

            var recordsTotal = await requests.CountAsync();
            var data = await requests.Skip(start).Take(length).Select(lr => new
            {
                lr.Id,
                EmployeeId = lr.Employee.Id,
                EmployeeName = lr.Employee.FullName,
                EmployeeCode = lr.Employee.Code,
                Type = lr.Type.ToString(),
                StartDate = lr.StartDate.ToString("yyyy-MM-dd"),
                EndDate = lr.EndDate.ToString("yyyy-MM-dd"),
                lr.Days,
                lr.Reason,
                Status = lr.Status.ToString(),
                lr.DecisionBy,
                DecisionAt = lr.DecisionAt.HasValue ? lr.DecisionAt.Value.ToString("yyyy-MM-dd HH:mm") : ""
            }).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: Reports/ExportLeaveCsv
        public async Task<IActionResult> ExportLeaveCsv(DateTime? startDate, DateTime? endDate, string statusFilter, string typeFilter)
        {
            var requests = _context.LeaveRequests
                                   .Include(lr => lr.Employee)
                                   .AsQueryable();

            if (startDate.HasValue)
            {
                requests = requests.Where(lr => lr.StartDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                requests = requests.Where(lr => lr.EndDate <= endDate.Value);
            }
            if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse(statusFilter, out LeaveStatus parsedStatus))
            {
                requests = requests.Where(lr => lr.Status == parsedStatus);
            }
            if (!string.IsNullOrEmpty(typeFilter) && Enum.TryParse(typeFilter, out LeaveTypeEnum parsedType))
            {
                requests = requests.Where(lr => lr.Type == parsedType);
            }

            var data = await requests.OrderBy(lr => lr.StartDate).ThenBy(lr => lr.Employee.FullName).ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Employee Code,Employee Name,Type,Start Date,End Date,Days,Reason,Status,Decision By,Decision At");

            foreach (var item in data)
            {
                sb.AppendLine($"{item.Employee.Code},{item.Employee.FullName},{item.Type},{item.StartDate:yyyy-MM-dd},{item.EndDate:yyyy-MM-dd},{item.Days},{item.Reason?.Replace(",", ";")},{item.Status},{item.DecisionBy?.Replace(",", ";")},{item.DecisionAt?.ToString("yyyy-MM-dd HH:mm")}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "LeaveReport.csv");
        }

        // GET: Reports/PayrollSummary
        public IActionResult PayrollSummary()
        {
            return View();
        }

        // GET: Reports/GetPayrollSummaryData
        public async Task<IActionResult> GetPayrollSummaryData(int? year, int? month)
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            var query = _context.PayrollItems
                                .Include(pi => pi.Employee)
                                .Include(pi => pi.PayrollRun)
                                .AsQueryable();

            if (year.HasValue && year.Value > 0)
            {
                query = query.Where(pi => pi.PayrollRun.Year == year.Value);
            }
            if (month.HasValue && month.Value > 0)
            {
                query = query.Where(pi => pi.PayrollRun.Month == month.Value);
            }

            // Group by PayrollRun to get summary
            var summary = query.GroupBy(pi => new { pi.PayrollRun.Id, pi.PayrollRun.Year, pi.PayrollRun.Month, pi.PayrollRun.RunAt, pi.PayrollRun.Notes })
                               .Select(g => new
                               {
                                   PayrollRunId = g.Key.Id,
                                   Year = g.Key.Year,
                                   Month = g.Key.Month,
                                   RunAt = g.Key.RunAt,
                                   Notes = g.Key.Notes,
                                   TotalBaseSalary = g.Sum(pi => pi.BaseSalary),
                                   TotalAllowances = g.Sum(pi => pi.Allowances),
                                   TotalDeductions = g.Sum(pi => pi.Deductions),
                                   TotalNetPay = g.Sum(pi => pi.NetPay),
                                   EmployeeCount = g.Count()
                               })
                               .AsQueryable();

            // Apply global search
            if (!string.IsNullOrEmpty(searchValue))
            {
                summary = summary.Where(s =>
                    s.Year.ToString().Contains(searchValue) ||
                    s.Month.ToString().Contains(searchValue) ||
                    s.Notes.Contains(searchValue));
            }

            // Sorting
            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
            if (!string.IsNullOrEmpty(sortColumn))
            {
                var propertyName = sortColumn switch
                {
                    "year" => "Year",
                    "month" => "Month",
                    "runAt" => "RunAt",
                    "totalBaseSalary" => "TotalBaseSalary",
                    "totalAllowances" => "TotalAllowances",
                    "totalDeductions" => "TotalDeductions",
                    "totalNetPay" => "TotalNetPay",
                    "employeeCount" => "EmployeeCount",
                    _ => null
                };

                if (propertyName != null)
                {
                    summary = sortColumnDir == "asc" ?
                        summary.OrderBy(s => EF.Property<object>(s, propertyName)) :
                        summary.OrderByDescending(s => EF.Property<object>(s, propertyName));
                }
                else
                {
                    summary = summary.OrderByDescending(s => s.RunAt); // Fallback
                }
            }
            else
            {
                summary = summary.OrderByDescending(s => s.RunAt); // Default sort
            }

            var recordsTotal = await summary.CountAsync();
            var data = await summary.Skip(start).Take(length).Select(s => new
            {
                s.PayrollRunId,
                s.Year,
                s.Month,
                RunAt = s.RunAt.ToString("yyyy-MM-dd HH:mm"),
                s.Notes,
                s.TotalBaseSalary,
                s.TotalAllowances,
                s.TotalDeductions,
                s.TotalNetPay,
                s.EmployeeCount
            }).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: Reports/ExportPayrollSummaryCsv
        public async Task<IActionResult> ExportPayrollSummaryCsv(int? year, int? month)
        {
            var query = _context.PayrollItems
                                .Include(pi => pi.Employee)
                                .Include(pi => pi.PayrollRun)
                                .AsQueryable();

            if (year.HasValue && year.Value > 0)
            {
                query = query.Where(pi => pi.PayrollRun.Year == year.Value);
            }
            if (month.HasValue && month.Value > 0)
            {
                query = query.Where(pi => pi.PayrollRun.Month == month.Value);
            }

            var summary = await query.GroupBy(pi => new { pi.PayrollRun.Id, pi.PayrollRun.Year, pi.PayrollRun.Month, pi.PayrollRun.RunAt, pi.PayrollRun.Notes })
                               .Select(g => new
                               {
                                   PayrollRunId = g.Key.Id,
                                   Year = g.Key.Year,
                                   Month = g.Key.Month,
                                   RunAt = g.Key.RunAt,
                                   Notes = g.Key.Notes,
                                   TotalBaseSalary = g.Sum(pi => pi.BaseSalary),
                                   TotalAllowances = g.Sum(pi => pi.Allowances),
                                   TotalDeductions = g.Sum(pi => pi.Deductions),
                                   TotalNetPay = g.Sum(pi => pi.NetPay),
                                   EmployeeCount = g.Count()
                               })
                               .OrderByDescending(s => s.RunAt)
                               .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Year,Month,Run At,Notes,Total Base Salary,Total Allowances,Total Deductions,Total Net Pay,Employee Count");

            foreach (var item in summary)
            {
                sb.AppendLine($"{item.Year},{item.Month},{item.RunAt:yyyy-MM-dd HH:mm},{item.Notes?.Replace(",", ";")},{item.TotalBaseSalary:N2},{item.TotalAllowances:N2},{item.TotalDeductions:N2},{item.TotalNetPay:N2},{item.EmployeeCount}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "PayrollSummaryReport.csv");
        }
    }
}
