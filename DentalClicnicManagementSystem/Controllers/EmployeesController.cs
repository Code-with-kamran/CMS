using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Controllers
{
    //[Authorize(Roles = "HR,Admin")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        public IActionResult Index()
        {
            return View();
        }

        // GET: Employees/GetEmployees
        public async Task<IActionResult> GetEmployees()
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            var employees = _context.Employees.AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                employees = employees.Where(e =>
                    e.FirstName.Contains(searchValue) ||
                    e.LastName.Contains(searchValue) ||
                    e.Email.Contains(searchValue) ||
                    e.Phone.Contains(searchValue) ||
                    e.Designation.ToString().Contains(searchValue) ||
                    e.Code.Contains(searchValue));
            }

            // Sorting
            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
            if (!string.IsNullOrEmpty(sortColumn))
            {
                employees = sortColumnDir == "asc" ?
                    employees.OrderBy(e => EF.Property<object>(e, sortColumn)) :
                    employees.OrderByDescending(e => EF.Property<object>(e, sortColumn));
            }
            else
            {
                // Default sort if no column is specified (e.g., by Id)
                employees = employees.OrderBy(e => e.Id);
            }

            var recordsTotal = await employees.CountAsync();
            var data = await employees.Skip(start).Take(length).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: Employees/Upsert
        public async Task<IActionResult> Upsert(int? id)
        {
            Employee employee = new Employee();
            if (id == null)
            {
                // Create
                return View(employee);
            }
            // Edit
            employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Employee employee)
        {
            if (ModelState.IsValid)
            {
                if (employee.Id == 0)
                {
                    // Check for duplicate Code or Email on create
                    if (await _context.Employees.AnyAsync(e => e.Code == employee.Code))
                    {
                        ModelState.AddModelError("Code", "Employee Code already exists.");
                        return View(employee);
                    }
                    if (await _context.Employees.AnyAsync(e => e.Email == employee.Email))
                    {
                        ModelState.AddModelError("Email", "Email address already exists.");
                        return View(employee);
                    }
                    _context.Employees.Add(employee);
                    TempData["success"] = "Employee created successfully!";
                }
                else
                {
                    // Check for duplicate Code or Email on update, excluding current employee
                    if (await _context.Employees.AnyAsync(e => e.Code == employee.Code && e.Id != employee.Id))
                    {
                        ModelState.AddModelError("Code", "Employee Code already exists.");
                        return View(employee);
                    }
                    if (await _context.Employees.AnyAsync(e => e.Email == employee.Email && e.Id != employee.Id))
                    {
                        ModelState.AddModelError("Email", "Email address already exists.");
                        return View(employee);
                    }
                    _context.Employees.Update(employee);
                    TempData["success"] = "Employee updated successfully!";
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return Json(new { status = false, message = "Employee not found." });
            }

            // Soft delete: Set IsActive to false
            employee.IsActive = false;
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Json(new { status = true, message = $"{employee.FullName} has been deactivated." });
        }
    }
}