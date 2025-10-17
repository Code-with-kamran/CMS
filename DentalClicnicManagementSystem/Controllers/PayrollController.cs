using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Controllers
{
    [Authorize(Roles = "HR,Admin")]
    public class PayrollController : Controller
    {
        private readonly ApplicationDbContext _context;

        

        public PayrollController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payroll
        public IActionResult Index()
        {
            return View();
        }

        // AJAX: Fetch payroll runs with datatables support
        // AJAX: Fetch payroll runs with datatables support
        [HttpGet]
        public async Task<IActionResult> GetPayrollRuns()
        {
            try
            {
                var draw = Request.Query["draw"].FirstOrDefault();
                var start = Request.Query["start"].FirstOrDefault();
                var length = Request.Query["length"].FirstOrDefault();
                var searchValue = Request.Query["search[value]"].FirstOrDefault();
                var sortColumnIndex = Request.Query["order[0][column]"].FirstOrDefault();
                var sortColumnDirection = Request.Query["order[0][dir]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var query = _context.PayrollRuns.AsQueryable();

                // Search filtering
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.Year.ToString().Contains(searchValue) ||
                        m.Month.ToString().Contains(searchValue) ||
                        m.Notes.Contains(searchValue) ||
                        m.RunAt.ToString().Contains(searchValue)
                    );
                }

                // Sorting
                if (!string.IsNullOrEmpty(sortColumnIndex))
                {
                    var sortColumn = Request.Query[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();

                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        query = sortColumn switch
                        {
                            "year" => sortColumnDirection == "asc" ? query.OrderBy(m => m.Year) : query.OrderByDescending(m => m.Year),
                            "month" => sortColumnDirection == "asc" ? query.OrderBy(m => m.Month) : query.OrderByDescending(m => m.Month),
                            "runAt" => sortColumnDirection == "asc" ? query.OrderBy(m => m.RunAt) : query.OrderByDescending(m => m.RunAt),
                            _ => query.OrderByDescending(m => m.RunAt)
                        };
                    }
                }
                else
                {
                    query = query.OrderByDescending(m => m.RunAt);
                }

                var recordsTotal = await query.CountAsync();

                var data = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        r.Id,
                        r.Year,
                        r.Month,
                        RunAt = r.RunAt.ToString("yyyy-MM-dd HH:mm"),
                        r.Notes
                    }).ToListAsync();

                // Calculate summary data for the cards
                var allRuns = await _context.PayrollRuns.ToListAsync();
                var totalRuns = allRuns.Count;

                // Calculate runs in last 7 days for percentage change
                var lastWeek = DateTime.Now.AddDays(-7);
                var runsLastWeek = allRuns.Count(r => r.RunAt >= lastWeek);
                var runsBeforeLastWeek = allRuns.Count(r => r.RunAt < lastWeek && r.RunAt >= lastWeek.AddDays(-7));

                decimal totalRunsChange = 0;
                if (runsBeforeLastWeek > 0)
                {
                    totalRunsChange = ((runsLastWeek - runsBeforeLastWeek) / (decimal)runsBeforeLastWeek) * 100;
                }
                else if (runsLastWeek > 0)
                {
                    totalRunsChange = 100; // 100% increase if no runs in previous period
                }

                // For pending actions, you might want to calculate based on your business logic
                // For now, let's assume pending actions are runs from current month that need review
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;
                var pendingActions = allRuns.Count(r => r.Year == currentYear && r.Month == currentMonth);

                var summary = new
                {
                    totalRuns = totalRuns,
                    pendingActions = pendingActions,
                    totalRunsChange = totalRunsChange >= 0 ? $"+{totalRunsChange:0}%" : $"{totalRunsChange:0}%",
                    pendingActionsChange = "+0%" // You can implement similar logic for pending actions
                };

                return Json(new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = data,
                    summary = summary // Add summary data for the cards
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }// GET: Create Run view
        public IActionResult CreateRun()
        {
            return View();
        }

        // POST: Generate payroll with attendance integration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePayrollRun(int year, int month, string notes)
        {
            try
            {
                if (year == 0 || month == 0)
                {
                    TempData["error"] = "Please select a valid year and month.";
                    return RedirectToAction(nameof(CreateRun));
                }

                // Check if payroll already exists
                if (await _context.PayrollRuns.AnyAsync(x => x.Year == year && x.Month == month))
                {
                    TempData["error"] = $"Payroll for {new DateTime(year, month, 1):MMMM yyyy} already exists.";
                    return RedirectToAction(nameof(CreateRun));
                }

                var newRun = new PayrollRun
                {
                    Year = year,
                    Month = month,
                    RunAt = DateTime.Now,
                    Notes = notes
                };
                _context.PayrollRuns.Add(newRun);
                await _context.SaveChangesAsync();

                // Get active employees with their attendance data
                var activeEmployees = await _context.Employees
                    .Where(e => e.IsActive)
                    .ToListAsync();

                var payrollItems = new List<PayrollItem>();
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                foreach (var emp in activeEmployees)
                {
                    // Calculate attendance-based metrics
                    var attendanceRecords = await _context.AttendanceRecords
                        .Where(a => a.EmployeeId == emp.Id &&
                                   a.Date >= startDate &&
                                   a.Date <= endDate)
                        .ToListAsync();

                    

                    // With these lines:
                    var totalWorkingDays = attendanceRecords.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.HalfDay);
                    var absentDays = attendanceRecords.Count(a => a.Status == AttendanceStatus.Absent);
                    var halfDays = attendanceRecords.Count(a => a.Status == AttendanceStatus.HalfDay);
                    // Calculate base salary (prorated based on attendance)
                    decimal monthlyBase = emp.BaseSalary / 12;
                    decimal dailyRate = monthlyBase / DateTime.DaysInMonth(year, month);
                    decimal baseSalaryEarned = dailyRate * totalWorkingDays + (dailyRate * 0.5m * halfDays);

                    // Calculate allowances
                    var overtimeHours = attendanceRecords.Sum(a => a.OvertimeHours ?? 0);
                    decimal housingAllowance = emp.HousingAllowance ?? 0;
                    decimal transportAllowance = emp.TransportAllowance ?? 0;
                    decimal overtimePay = (emp.BaseSalary / (22 * 8)) * 1.5m * overtimeHours; // 1.5x overtime rate
                    decimal totalAllowances = housingAllowance + transportAllowance + overtimePay + 500; // Base allowance

                    // Calculate deductions
                    decimal taxDeduction = CalculateTax(baseSalaryEarned + totalAllowances);
                    decimal socialSecurity = (baseSalaryEarned + totalAllowances) * 0.05m; // 5% social security
                    decimal absenceDeduction = dailyRate * absentDays;
                    decimal totalDeductions = taxDeduction + socialSecurity + absenceDeduction + 100; // Base deduction

                    decimal netPay = baseSalaryEarned + totalAllowances - totalDeductions;

                    payrollItems.Add(new PayrollItem
                    {
                        PayrollRunId = newRun.Id,
                        EmployeeId = emp.Id,
                        BaseSalary = Math.Round(baseSalaryEarned, 2),
                        Allowances = Math.Round(totalAllowances, 2),
                        Deductions = Math.Round(totalDeductions, 2),
                        NetPay = Math.Round(netPay, 2),
                        AttendanceSummary = $"Present: {totalWorkingDays}, Absent: {absentDays}, Half-days: {halfDays}, Overtime: {overtimeHours}hrs"
                    });
                }

                _context.PayrollItems.AddRange(payrollItems);
                await _context.SaveChangesAsync();

                TempData["success"] = $"Payroll for {new DateTime(year, month, 1):MMMM yyyy} generated successfully! Processed {payrollItems.Count} employees.";
                return RedirectToAction(nameof(RunDetails), new { id = newRun.Id });
            }
            catch (Exception ex)
            {
                TempData["error"] = $"Error generating payroll: {ex.Message}";
                return RedirectToAction(nameof(CreateRun));
            }
        }

        private decimal CalculateTax(decimal grossSalary)
        {
            // Simplified tax calculation (adjust based on your tax brackets)
            if (grossSalary <= 1000) return 0;
            if (grossSalary <= 2000) return (grossSalary - 1000) * 0.1m;
            if (grossSalary <= 3000) return 100 + (grossSalary - 2000) * 0.15m;
            return 250 + (grossSalary - 3000) * 0.2m;
        }

        // GET: Run Details

        public async Task<IActionResult> RunDetails(int id)
        {
            var run = await _context.PayrollRuns.FindAsync(id);
            if (run == null)
                return NotFound();

            var payrollItems = await _context.PayrollItems.Where(x => x.PayrollRunId == id).ToListAsync();

            int prevYear = run.Month == 1 ? run.Year - 1 : run.Year;
            int prevMonth = run.Month == 1 ? 12 : run.Month - 1;
            var prevRun = await _context.PayrollRuns.FirstOrDefaultAsync(r => r.Year == prevYear && r.Month == prevMonth);

            List<PayrollItem> prevItems = new();
            if (prevRun != null)
                prevItems = await _context.PayrollItems.Where(x => x.PayrollRunId == prevRun.Id).ToListAsync();

            int empNow = payrollItems.Count;
            int empPrev = prevItems.Count;
            decimal netPayNow = payrollItems.Sum(x => x.NetPay);
            decimal netPayPrev = prevItems.Sum(x => x.NetPay);
            decimal allowNow = payrollItems.Sum(x => x.Allowances);
            decimal allowPrev = prevItems.Sum(x => x.Allowances);
            decimal dedNow = payrollItems.Sum(x => x.Deductions);
            decimal dedPrev = prevItems.Sum(x => x.Deductions);

            decimal Change(decimal cur, decimal prev) =>
                prev == 0 ? (cur == 0 ? 0 : 100) : Math.Round((cur - prev) / Math.Abs(prev) * 100, 2);

            var summary = new PayrollRunCardSummary
            {
                TotalEmployees = empNow,
                TotalEmployeesChangePercent = Change(empNow, empPrev),
                TotalNetPay = netPayNow,
                TotalNetPayChangePercent = Change(netPayNow, netPayPrev),
                TotalAllowances = allowNow,
                TotalAllowancesChangePercent = Change(allowNow, allowPrev),
                TotalDeductions = dedNow,
                TotalDeductionsChangePercent = Change(dedNow, dedPrev)
            };

            var vm = new PayrollRunDetailsViewModel
            {
                PayrollRun = run,
                CardSummary = summary
            };

            return View(vm);
        }

        //public async Task<IActionResult> RunDetails(int id)
        //{
        //    var run = await _context.PayrollRuns.FindAsync(id);
        //    if (run == null)
        //        return NotFound();
        //    return View(run);
        //}

        // AJAX: Fetch payroll items with datatables support
        [HttpGet]
        public async Task<IActionResult> GetPayrollItems(int runId)
        {
            try
            {
                var draw = Request.Query["draw"].FirstOrDefault();
                var start = Request.Query["start"].FirstOrDefault();
                var length = Request.Query["length"].FirstOrDefault();
                var searchValue = Request.Query["search[value]"].FirstOrDefault();
                var sortColumnIndex = Request.Query["order[0][column]"].FirstOrDefault();
                var sortColumnDirection = Request.Query["order[0][dir]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var query = _context.PayrollItems
                    .Include(x => x.Employee)
                    .Where(x => x.PayrollRunId == runId)
                    .AsQueryable();

                // Search filtering
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.Employee.FirstName.Contains(searchValue) ||
                        m.Employee.LastName.Contains(searchValue) ||
                        m.Employee.Code.Contains(searchValue)
                    );
                }

                // Sorting
                if (!string.IsNullOrEmpty(sortColumnIndex))
                {
                    var sortColumn = Request.Query[$"columns[{sortColumnIndex}][data]"].FirstOrDefault();

                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        query = sortColumn switch
                        {
                            "employeeCode" => sortColumnDirection == "asc" ? query.OrderBy(m => m.Employee.Code) : query.OrderByDescending(m => m.Employee.Code),
                            "employeeName" => sortColumnDirection == "asc" ? query.OrderBy(m => m.Employee.FirstName) : query.OrderByDescending(m => m.Employee.FirstName),
                            "baseSalary" => sortColumnDirection == "asc" ? query.OrderBy(m => m.BaseSalary) : query.OrderByDescending(m => m.BaseSalary),
                            "allowances" => sortColumnDirection == "asc" ? query.OrderBy(m => m.Allowances) : query.OrderByDescending(m => m.Allowances),
                            "deductions" => sortColumnDirection == "asc" ? query.OrderBy(m => m.Deductions) : query.OrderByDescending(m => m.Deductions),
                            "netPay" => sortColumnDirection == "asc" ? query.OrderBy(m => m.NetPay) : query.OrderByDescending(m => m.NetPay),
                            _ => query.OrderBy(m => m.Employee.FirstName)
                        };
                    }
                }
                else
                {
                    query = query.OrderBy(m => m.Employee.FirstName);
                }

                var recordsTotal = await query.CountAsync();

                var data = await query
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(x => new {
                        x.Id,
                        EmployeeName = x.Employee.FullName,
                        EmployeeCode = x.Employee.Code,
                        Designation = x.Employee.Designation,
                        x.BaseSalary,
                        x.Allowances,
                        x.Deductions,
                        x.NetPay,
                        x.AttendanceSummary
                    }).ToListAsync();

                return Json(new
                {
                    draw = draw,
                    recordsFiltered = recordsTotal,
                    recordsTotal = recordsTotal,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }// GET: Payslip
        public async Task<IActionResult> Payslip(int id)
        {
            var item = await _context.PayrollItems
                .Include(pi => pi.Employee)
                .Include(pi => pi.PayrollRun)
                .FirstOrDefaultAsync(pi => pi.Id == id);

            if (item == null)
                return NotFound();

            return View(item);
        }

        // AJAX: Recalculate payroll item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecalculateItem(int id, decimal allowances, decimal deductions)
        {
            try
            {
                var item = await _context.PayrollItems
                    .Include(pi => pi.Employee)
                    .FirstOrDefaultAsync(pi => pi.Id == id);

                if (item == null)
                    return Json(new { status = false, message = "Payroll item not found." });

                item.Allowances = allowances;
                item.Deductions = deductions;
                item.NetPay = item.BaseSalary + allowances - deductions;

                _context.PayrollItems.Update(item);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    status = true,
                    message = "Payroll item recalculated successfully!",
                    netPay = item.NetPay.ToString("N2"),
                    allowances = item.Allowances.ToString("N2"),
                    deductions = item.Deductions.ToString("N2")
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"Error: {ex.Message}" });
            }
        }

        // Export to CSV
        public async Task<IActionResult> ExportPayrollItemsCsv(int id)
{
    var items = await _context.PayrollItems
        .Include(pi => pi.Employee)
        .Include(pi => pi.PayrollRun)
        .Where(pi => pi.PayrollRunId == id)
        .ToListAsync();

    var run = await _context.PayrollRuns.FindAsync(id);
    var filename = $"Payroll_Run_{run?.Year}_{run?.Month:00}.csv";

    var sb = new StringBuilder();
    sb.AppendLine("Employee Code,Employee Name,Designation,Base Salary,Allowances,Deductions,Net Pay,Attendance Summary");

    foreach (var item in items)
    {
        sb.AppendLine($"\"{item.Employee.Code}\",\"{item.Employee.FullName}\",\"{item.Employee.Designation}\",{item.BaseSalary:N2},{item.Allowances:N2},{item.Deductions:N2},{item.NetPay:N2},\"{item.AttendanceSummary}\"");
    }

    return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", filename);
}

        // GET: Payroll Summary Report
        public async Task<IActionResult> SummaryReport(int id)
        {
            var run = await _context.PayrollRuns.FindAsync(id);
            if (run == null)
                return NotFound();

            var payrollItems = await _context.PayrollItems
                .Where(pi => pi.PayrollRunId == id)
                .ToListAsync();

            var viewModel = new PayrollSummaryViewModel
            {
                PayrollRun = run,
                TotalEmployees = payrollItems.Count,
                TotalBaseSalary = payrollItems.Sum(pi => pi.BaseSalary),
                TotalAllowances = payrollItems.Sum(pi => pi.Allowances),
                TotalDeductions = payrollItems.Sum(pi => pi.Deductions),
                TotalNetPay = payrollItems.Sum(pi => pi.NetPay),
                AverageNetPay = payrollItems.Count > 0 ? payrollItems.Average(pi => pi.NetPay) : 0
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<JsonResult> GetPayrollCardSummary(int runId)
        {
            var run = await _context.PayrollRuns.FindAsync(runId);
            if (run == null)
                return Json(new { success = false });

            var payrollItems = await _context.PayrollItems
                .Where(x => x.PayrollRunId == runId).ToListAsync();

            int prevYear = run.Month == 1 ? run.Year - 1 : run.Year;
            int prevMonth = run.Month == 1 ? 12 : run.Month - 1;
            var prevRun = await _context.PayrollRuns.FirstOrDefaultAsync(r => r.Year == prevYear && r.Month == prevMonth);
            var prevItems = prevRun != null
                ? await _context.PayrollItems.Where(x => x.PayrollRunId == prevRun.Id).ToListAsync()
                : new List<PayrollItem>();

            decimal Change(decimal cur, decimal prev) =>
                prev == 0 ? (cur == 0 ? 0 : 100) : Math.Round((cur - prev) / Math.Abs(prev) * 100, 2);

            var summary = new
            {
                TotalEmployees = payrollItems.Count,
                TotalEmployeesChangePercent = Change(payrollItems.Count, prevItems.Count),
                TotalNetPay = payrollItems.Sum(x => x.NetPay),
                TotalNetPayChangePercent = Change(payrollItems.Sum(x => x.NetPay), prevItems.Sum(x => x.NetPay)),
                TotalAllowances = payrollItems.Sum(x => x.Allowances),
                TotalAllowancesChangePercent = Change(payrollItems.Sum(x => x.Allowances), prevItems.Sum(x => x.Allowances)),
                TotalDeductions = payrollItems.Sum(x => x.Deductions),
                TotalDeductionsChangePercent = Change(payrollItems.Sum(x => x.Deductions), prevItems.Sum(x => x.Deductions))
            };

            return Json(new { success = true, data = summary });
        }

        // POST: Payroll/DeleteRun/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRun(int id)
        {
            try
            {
                var run = await _context.PayrollRuns
                    .Include(r => r.Items)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (run == null)
                {
                    return Json(new { status = false, message = "Payroll run not found." });
                }

                // Remove associated payroll items first
                _context.PayrollItems.RemoveRange(run.Items);
                _context.PayrollRuns.Remove(run);

                await _context.SaveChangesAsync();

                return Json(new
                {
                    status = true,
                    message = $"Payroll run for {new DateTime(run.Year, run.Month, 1):MMMM yyyy} deleted successfully!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = $"Error deleting payroll run: {ex.Message}" });
            }
        }
    }
}