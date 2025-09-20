// File: Controllers/PayrollController.cs
using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace CMS.Controllers
{
    //[Authorize(Roles = "HR,Admin")]
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

        // GET: Payroll/GetPayrollRuns
        public async Task<IActionResult> GetPayrollRuns()
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            var runs = _context.PayrollRuns.AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                runs = runs.Where(r =>
                    r.Notes.Contains(searchValue) ||
                    r.Year.ToString().Contains(searchValue) ||
                    r.Month.ToString().Contains(searchValue));
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
                    "notes" => "Notes",
                    _ => null
                };

                if (propertyName != null)
                {
                    runs = sortColumnDir == "asc" ?
                        runs.OrderBy(r => EF.Property<object>(r, propertyName)) :
                        runs.OrderByDescending(r => EF.Property<object>(r, propertyName));
                }
                else
                {
                    runs = runs.OrderByDescending(r => r.RunAt); // Fallback
                }
            }
            else
            {
                runs = runs.OrderByDescending(r => r.RunAt); // Default sort
            }

            var recordsTotal = await runs.CountAsync();
            var data = await runs.Skip(start).Take(length).Select(r => new
            {
                r.Id,
                r.Year,
                r.Month,
                RunAt = r.RunAt.ToString("yyyy-MM-dd HH:mm"),
                r.Notes
            }).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: Payroll/CreateRun
        public IActionResult CreateRun()
        {
            return View();
        }

        // POST: Payroll/GeneratePayrollRun
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GeneratePayrollRun(int year, int month, string notes)
        {
            if (year == 0 || month == 0)
            {
                TempData["error"] = "Please select a valid year and month.";
                return RedirectToAction(nameof(CreateRun));
            }

            // Check if payroll already exists for this month/year
            if (await _context.PayrollRuns.AnyAsync(pr => pr.Year == year && pr.Month == month))
            {
                TempData["error"] = $"Payroll for {new DateTime(year, month, 1).ToString("MMMM yyyy")} already exists.";
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
            await _context.SaveChangesAsync(); // Save to get the newRun.Id

            var activeEmployees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
            var payrollItems = new List<PayrollItem>();

            foreach (var employee in activeEmployees)
            {
                // Simple calculation: Monthly BaseSalary + fixed allowances - fixed deductions
                var monthlyBaseSalary = employee.BaseSalary / 12;
                var allowances = 500.00m; // Example fixed allowance
                var deductions = 100.00m; // Example fixed deduction

                var netPay = monthlyBaseSalary + allowances - deductions;

                payrollItems.Add(new PayrollItem
                {
                    PayrollRunId = newRun.Id,
                    EmployeeId = employee.Id,
                    BaseSalary = monthlyBaseSalary,
                    Allowances = allowances,
                    Deductions = deductions,
                    NetPay = netPay
                });
            }

            _context.PayrollItems.AddRange(payrollItems);
            await _context.SaveChangesAsync();

            TempData["success"] = $"Payroll for {new DateTime(year, month, 1).ToString("MMMM yyyy")} generated successfully!";
            return RedirectToAction(nameof(RunDetails), new { id = newRun.Id });
        }

        // GET: Payroll/RunDetails/5
        public async Task<IActionResult> RunDetails(int id)
        {
            var run = await _context.PayrollRuns.FindAsync(id);
            if (run == null)
            {
                return NotFound();
            }
            return View(run);
        }

        // GET: Payroll/GetPayrollItems/5
        public async Task<IActionResult> GetPayrollItems(int runId)
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            var items = _context.PayrollItems
                                .Include(pi => pi.Employee)
                                .Where(pi => pi.PayrollRunId == runId)
                                .AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                items = items.Where(pi =>
                    pi.Employee.FirstName.Contains(searchValue) ||
                    pi.Employee.LastName.Contains(searchValue) ||
                    pi.Employee.Code.Contains(searchValue));
            }

            // Sorting
            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
            if (!string.IsNullOrEmpty(sortColumn))
            {
                if (sortColumn == "employeeName")
                {
                    items = sortColumnDir == "asc" ?
                        items.OrderBy(pi => pi.Employee.FirstName).ThenBy(pi => pi.Employee.LastName) :
                        items.OrderByDescending(pi => pi.Employee.FirstName).ThenByDescending(pi => pi.Employee.LastName);
                }
                else if (sortColumn == "employeeCode")
                {
                    items = sortColumnDir == "asc" ?
                        items.OrderBy(pi => pi.Employee.Code) :
                        items.OrderByDescending(pi => pi.Employee.Code);
                }
                else
                {
                    var propertyName = sortColumn switch
                    {
                        "baseSalary" => "BaseSalary",
                        "allowances" => "Allowances",
                        "deductions" => "Deductions",
                        "netPay" => "NetPay",
                        _ => null
                    };

                    if (propertyName != null)
                    {
                        items = sortColumnDir == "asc" ?
                            items.OrderBy(pi => EF.Property<object>(pi, propertyName)) :
                            items.OrderByDescending(pi => EF.Property<object>(pi, propertyName));
                    }
                    else
                    {
                        items = items.OrderBy(pi => pi.Employee.FirstName); // Fallback
                    }
                }
            }
            else
            {
                items = items.OrderBy(pi => pi.Employee.FirstName); // Default sort
            }

            var recordsTotal = await items.CountAsync();
            var data = await items.Skip(start).Take(length).Select(pi => new
            {
                pi.Id,
                EmployeeId = pi.Employee.Id,
                EmployeeName = pi.Employee.FullName,
                EmployeeCode = pi.Employee.Code,
                pi.BaseSalary,
                pi.Allowances,
                pi.Deductions,
                pi.NetPay
            }).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: Payroll/Payslip/5
        public async Task<IActionResult> Payslip(int id)
        {
            var item = await _context.PayrollItems
                                     .Include(pi => pi.Employee)
                                     .Include(pi => pi.PayrollRun)
                                     .FirstOrDefaultAsync(pi => pi.Id == id);
            if (item == null)
            {
                return NotFound();
            }
            return View(item);
        }

        // POST: Payroll/RecalculateItem/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecalculateItem(int id, decimal allowances, decimal deductions)
        {
            var item = await _context.PayrollItems.FindAsync(id);
            if (item == null)
            {
                return Json(new { status = false, message = "Payroll item not found." });
            }

            item.Allowances = allowances;
            item.Deductions = deductions;
            item.NetPay = item.BaseSalary + item.Allowances - item.Deductions;

            _context.PayrollItems.Update(item);
            await _context.SaveChangesAsync();

            return Json(new { status = true, message = "Payroll item recalculated successfully!", netPay = item.NetPay.ToString("N2") });
        }

        // GET: Payroll/ExportPayrollItemsCsv/5
        public async Task<IActionResult> ExportPayrollItemsCsv(int runId)
        {
            var items = await _context.PayrollItems
                                      .Include(pi => pi.Employee)
                                      .Where(pi => pi.PayrollRunId == runId)
                                      .ToListAsync();

            var run = await _context.PayrollRuns.FindAsync(runId);
            var filename = $"Payroll_Run_{run?.Year}_{run?.Month}.csv";

            var sb = new StringBuilder();
            sb.AppendLine("Employee Code,Employee Name,Base Salary,Allowances,Deductions,Net Pay");

            foreach (var item in items)
            {
                sb.AppendLine($"{item.Employee.Code},{item.Employee.FullName},{item.BaseSalary:N2},{item.Allowances:N2},{item.Deductions:N2},{item.NetPay:N2}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", filename);
        }
    }
}

