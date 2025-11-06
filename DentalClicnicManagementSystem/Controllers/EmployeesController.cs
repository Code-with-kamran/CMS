////using CMS.Data;
////using CMS.Models;
////using Microsoft.AspNetCore.Authorization;
////using Microsoft.AspNetCore.Mvc;
////using Microsoft.EntityFrameworkCore;
////using System.Linq;
////using System.Threading.Tasks;

////namespace CMS.Controllers
////{
////    //[Authorize(Roles = "HR,Admin")]
////    public class EmployeesController : Controller
////    {
////        private readonly ApplicationDbContext _context;

////        public EmployeesController(ApplicationDbContext context)
////        {
////            _context = context;
////        }

////        // GET: Employees
////        public IActionResult Index()
////        {
////            return View();
////        }

////        // GET: Employees/GetEmployees
////        public async Task<IActionResult> GetEmployees()
////        {
////            var draw = HttpContext.Request.Query["draw"].ToString();
////            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
////            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
////            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
////            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
////            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

////            var employees = _context.Employees.AsQueryable();

////            // Filtering
////            if (!string.IsNullOrEmpty(searchValue))
////            {
////                employees = employees.Where(e =>
////                    e.FirstName.Contains(searchValue) ||
////                    e.LastName.Contains(searchValue) ||
////                    e.Email.Contains(searchValue) ||
////                    e.Phone.Contains(searchValue) ||
////                    e.Designation.ToString().Contains(searchValue) ||
////                    e.Code.Contains(searchValue));
////            }

////            // Sorting
////            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
////            if (!string.IsNullOrEmpty(sortColumn))
////            {
////                employees = sortColumnDir == "asc" ?
////                    employees.OrderBy(e => EF.Property<object>(e, sortColumn)) :
////                    employees.OrderByDescending(e => EF.Property<object>(e, sortColumn));
////            }
////            else
////            {
////                // Default sort if no column is specified (e.g., by Id)
////                employees = employees.OrderBy(e => e.Id);
////            }

////            var recordsTotal = await employees.CountAsync();
////            var data = await employees.Skip(start).Take(length).ToListAsync();

////            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
////        }

////        // GET: Employees/Upsert
////        public async Task<IActionResult> Upsert(int? id)
////        {
////            Employee employee = new Employee();
////            if (id == null)
////            {
////                // Create
////                return View(employee);
////            }
////            // Edit
////            employee = await _context.Employees.FindAsync(id);
////            if (employee == null)
////            {
////                return NotFound();
////            }
////            return View(employee);
////        }

////        // POST: Employees/Upsert
////        [HttpPost]
////        [ValidateAntiForgeryToken]
////        public async Task<IActionResult> Upsert(Employee employee)
////        {
////            if (ModelState.IsValid)
////            {
////                if (employee.Id == 0)
////                {
////                    // Check for duplicate Code or Email on create
////                    if (await _context.Employees.AnyAsync(e => e.Code == employee.Code))
////                    {
////                        ModelState.AddModelError("Code", "Employee Code already exists.");
////                        return View(employee);
////                    }
////                    if (await _context.Employees.AnyAsync(e => e.Email == employee.Email))
////                    {
////                        ModelState.AddModelError("Email", "Email address already exists.");
////                        return View(employee);
////                    }
////                    _context.Employees.Add(employee);
////                    TempData["success"] = "Employee created successfully!";
////                }
////                else
////                {
////                    // Check for duplicate Code or Email on update, excluding current employee
////                    if (await _context.Employees.AnyAsync(e => e.Code == employee.Code && e.Id != employee.Id))
////                    {
////                        ModelState.AddModelError("Code", "Employee Code already exists.");
////                        return View(employee);
////                    }
////                    if (await _context.Employees.AnyAsync(e => e.Email == employee.Email && e.Id != employee.Id))
////                    {
////                        ModelState.AddModelError("Email", "Email address already exists.");
////                        return View(employee);
////                    }
////                    _context.Employees.Update(employee);
////                    TempData["success"] = "Employee updated successfully!";
////                }
////                await _context.SaveChangesAsync();
////                return RedirectToAction(nameof(Index));
////            }
////            return View(employee);
////        }

////        // POST: Employees/Delete/5
////        [HttpPost]
////        [ValidateAntiForgeryToken]
////        public async Task<IActionResult> Delete(int id)
////        {
////            try
////            {
////                var employee = await _context.Employees.FindAsync(id);
////                if (employee == null)
////                {
////                    return Json(new { status = false, message = "Employee not found." });
////                }

////                // Soft delete: Set IsActive to false
////                employee.IsActive = false;
////                _context.Employees.Update(employee);
////                await _context.SaveChangesAsync();

////                return Json(new { status = true, message = $"{employee.FullName} has been deactivated." });
////            }
////            catch(Exception ex) {
////                return Json(new { status = false, message = $"{ex.Message}" });
////            }
////        }
////    }
////}


//using CMS.Data;
//using CMS.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Linq;
//using System.Threading.Tasks;

//namespace CMS.Controllers
//{
//    //[Authorize(Roles = "HR,Admin")]
//    public class EmployeesController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public EmployeesController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // GET: Employees
//        public IActionResult Index()
//        {
//            return View();
//        }

//        // GET: Employees/Deleted
//        public IActionResult Deleted()
//        {
//            return View();
//        }

//        // GET: Employees/GetEmployees
//        public async Task<IActionResult> GetEmployees()
//        {
//            var draw = HttpContext.Request.Query["draw"].ToString();
//            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
//            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
//            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
//            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
//            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

//            // Only get active employees
//            var employees = _context.Employees.Where(e => e.IsActive).AsQueryable();

//            // Filtering
//            if (!string.IsNullOrEmpty(searchValue))
//            {
//                employees = employees.Where(e =>
//                    e.FirstName.Contains(searchValue) ||
//                    e.LastName.Contains(searchValue) ||
//                    e.Email.Contains(searchValue) ||
//                    e.Phone.Contains(searchValue) ||
//                    e.Designation.ToString().Contains(searchValue) ||
//                    e.Code.Contains(searchValue));
//            }

//            // Sorting
//            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
//            if (!string.IsNullOrEmpty(sortColumn))
//            {
//                employees = sortColumnDir == "asc" ?
//                    employees.OrderBy(e => EF.Property<object>(e, sortColumn)) :
//                    employees.OrderByDescending(e => EF.Property<object>(e, sortColumn));
//            }
//            else
//            {
//                // Default sort if no column is specified (e.g., by Id)
//                employees = employees.OrderBy(e => e.Id);
//            }

//            var recordsTotal = await employees.CountAsync();
//            var data = await employees.Skip(start).Take(length).ToListAsync();

//            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
//        }

//        // GET: Employees/GetDeletedEmployees
//        public async Task<IActionResult> GetDeletedEmployees()
//        {
//            var draw = HttpContext.Request.Query["draw"].ToString();
//            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
//            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
//            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
//            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
//            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

//            // Only get inactive (deleted) employees
//            var employees = _context.Employees.Where(e => !e.IsActive).AsQueryable();

//            // Filtering
//            if (!string.IsNullOrEmpty(searchValue))
//            {
//                employees = employees.Where(e =>
//                    e.FirstName.Contains(searchValue) ||
//                    e.LastName.Contains(searchValue) ||
//                    e.Email.Contains(searchValue) ||
//                    e.Phone.Contains(searchValue) ||
//                    e.Designation.ToString().Contains(searchValue) ||
//                    e.Code.Contains(searchValue));
//            }

//            // Sorting
//            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
//            if (!string.IsNullOrEmpty(sortColumn))
//            {
//                employees = sortColumnDir == "asc" ?
//                    employees.OrderBy(e => EF.Property<object>(e, sortColumn)) :
//                    employees.OrderByDescending(e => EF.Property<object>(e, sortColumn));
//            }
//            else
//            {
//                // Default sort by deactivation date (you might want to add a DeactivatedDate property)
//                employees = employees.OrderByDescending(e => e.Id);
//            }

//            var recordsTotal = await employees.CountAsync();
//            var data = await employees.Skip(start).Take(length).ToListAsync();

//            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
//        }

//        // GET: Employees/Upsert
//        public async Task<IActionResult> Upsert(int? id)
//        {
//            Employee employee = new Employee();
//            if (id == null)
//            {
//                // Create
//                return View(employee);
//            }
//            // Edit
//            employee = await _context.Employees.FindAsync(id);
//            if (employee == null)
//            {
//                return NotFound();
//            }
//            return View(employee);
//        }

//        // POST: Employees/Upsert
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Upsert(Employee employee)
//        {
//            if (ModelState.IsValid)
//            {
//                if (employee.Id == 0)
//                {
//                    // Check for duplicate Code or Email on create
//                    if (await _context.Employees.AnyAsync(e => e.Code == employee.Code))
//                    {
//                        ModelState.AddModelError("Code", "Employee Code already exists.");
//                        return View(employee);
//                    }
//                    if (await _context.Employees.AnyAsync(e => e.Email == employee.Email))
//                    {
//                        ModelState.AddModelError("Email", "Email address already exists.");
//                        return View(employee);
//                    }
//                    _context.Employees.Add(employee);
//                    TempData["success"] = "Employee created successfully!";
//                }
//                else
//                {
//                    // Check for duplicate Code or Email on update, excluding current employee
//                    if (await _context.Employees.AnyAsync(e => e.Code == employee.Code && e.Id != employee.Id))
//                    {
//                        ModelState.AddModelError("Code", "Employee Code already exists.");
//                        return View(employee);
//                    }
//                    if (await _context.Employees.AnyAsync(e => e.Email == employee.Email && e.Id != employee.Id))
//                    {
//                        ModelState.AddModelError("Email", "Email address already exists.");
//                        return View(employee);
//                    }
//                    _context.Employees.Update(employee);
//                    TempData["success"] = "Employee updated successfully!";
//                }
//                await _context.SaveChangesAsync();
//                return RedirectToAction(nameof(Index));
//            }
//            return View(employee);
//        }

//        // POST: Employees/Delete/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Delete(int id)
//        {
//            try
//            {
//                var employee = await _context.Employees.FindAsync(id);
//                if (employee == null)
//                {
//                    return Json(new { status = false, message = "Employee not found." });
//                }

//                // Soft delete: Set IsActive to false
//                employee.IsActive = false;
//                _context.Employees.Update(employee);
//                await _context.SaveChangesAsync();

//                return Json(new { status = true, message = $"{employee.FirstName} {employee.LastName} has been deactivated." });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { status = false, message = $"An error occurred: {ex.Message}" });
//            }
//        }

//        // POST: Employees/Restore/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Restore(int id)
//        {
//            try
//            {
//                var employee = await _context.Employees.FindAsync(id);
//                if (employee == null)
//                {
//                    return Json(new { status = false, message = "Employee not found." });
//                }

//                // Restore: Set IsActive to true
//                employee.IsActive = true;
//                _context.Employees.Update(employee);
//                await _context.SaveChangesAsync();

//                return Json(new { status = true, message = $"{employee.FirstName} {employee.LastName} has been restored successfully." });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { status = false, message = $"An error occurred: {ex.Message}" });
//            }
//        }

//        // POST: Employees/PermanentDelete/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> PermanentDelete(int id)
//        {
//            try
//            {
//                var employee = await _context.Employees.FindAsync(id);
//                if (employee == null)
//                {
//                    return Json(new { status = false, message = "Employee not found." });
//                }

//                // Check if employee has any related records
//                bool hasAttendanceRecords = await _context.AttendanceRecords.AnyAsync(ar => ar.EmployeeId == id);
//                bool hasPerformanceReviews = await _context.PerformanceReviews.AnyAsync(pr => pr.EmployeeId == id);

//                if (hasAttendanceRecords || hasPerformanceReviews)
//                {
//                    return Json(new
//                    {
//                        status = false,
//                        message = "Cannot permanently delete employee because they have related records (attendance, performance reviews, etc.)."
//                    });
//                }

//                // Permanent delete (only if no related records)
//                _context.Employees.Remove(employee);
//                await _context.SaveChangesAsync();

//                return Json(new { status = true, message = $"{employee.FirstName} {employee.LastName} has been permanently deleted." });
//            }
//            catch (DbUpdateException ex)
//            {
//                return Json(new { status = false, message = "Cannot delete employee because they have related records in the system." });
//            }
//            catch (Exception ex)
//            {
//                return Json(new { status = false, message = $"An error occurred: {ex.Message}" });
//            }
//        }
//    }
//}


using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
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

        // GET: Employees/GetEmployees (Active employees only)
        public async Task<IActionResult> GetEmployees()
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            // Only get active employees
            var employees = _context.Employees.Where(e => e.IsActive && e.IsDeleted).AsQueryable();

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
                // Default sort if no column is specified
                employees = employees.OrderBy(e => e.Id);
            }

            var recordsTotal = await employees.CountAsync();
            var data = await employees.Skip(start).Take(length).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }
        // GET: Employees/GetAllEmployees (with status filter)
        // GET: Employees/GetAllEmployees (with status filter)

        [HttpGet]
        public async Task<IActionResult> ExportEmployees([FromQuery] string status = "active")
        {
            try
            {
                var query = _context.Employees.AsQueryable();

                // Apply status filter
                if (status == "active")
                {
                    query = query.Where(e => e.IsActive);
                }
                else if (status == "deactivated")
                {
                    query = query.Where(e => !e.IsActive);
                }

                var employees = await query
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .Select(e => new
                    {
                        e.Code,
                        e.FirstName,
                        e.LastName,
                        e.Email,
                        e.Phone,
                        e.Designation,
                        e.HireDate,
                        e.BaseSalary,
                        e.IsActive
                    })
                    .ToListAsync();

                // Build CSV
                var csv = new StringBuilder();
                csv.AppendLine("Code,Name,Email,Phone,Designation,Hire Date,Salary,Status");

                foreach (var emp in employees)
                {
                    var name = $"{emp.FirstName} {emp.LastName}";
                    var hireDate = emp.HireDate.ToString("dd-MM-yyyy") ?? "";
                    var salary = $"£{emp.BaseSalary:F2}";
                    var statusText = emp.IsActive ? "Active" : "Inactive";

                    csv.AppendLine($"{EscapeCsv(emp.Code)},{EscapeCsv(name)},{EscapeCsv(emp.Email)},{EscapeCsv(emp.Phone)},{EscapeCsv(emp.Designation)},{EscapeCsv(hireDate)},{EscapeCsv(salary)},{EscapeCsv(statusText)}");
                }

                var bytes = Encoding.UTF8.GetBytes(csv.ToString());
                var fileName = $"{(status == "active" ? "Active" : "Deactivated")}_Employees_{DateTime.Now:yyyyMMdd}.csv";

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, "Error generating CSV export");
            }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
        public async Task<IActionResult> GetAllEmployees()
        {
            try
            {
                var draw = HttpContext.Request.Query["draw"].FirstOrDefault();
                var start = int.Parse(HttpContext.Request.Query["start"].FirstOrDefault() ?? "0");
                var length = int.Parse(HttpContext.Request.Query["length"].FirstOrDefault() ?? "10");
                var searchValue = HttpContext.Request.Query["search[value]"].FirstOrDefault() ?? "";
                var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].FirstOrDefault() ?? "0");
                var sortColumnDirection = HttpContext.Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";
                var statusFilter = HttpContext.Request.Query["status"].FirstOrDefault() ?? "active";

                // Base query
                var employees = _context.Employees.AsQueryable();

                // Apply status filter
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    if (statusFilter == "active")
                    {
                        employees = employees.Where(e => e.IsActive);
                    }
                    else if (statusFilter == "deactivated")
                    {
                        employees = employees.Where(e => !e.IsActive);
                    }
                }

                // Total records count (before filtering)
                var totalRecords = await employees.CountAsync();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchValue))
                {
                    employees = employees.Where(e =>
                        e.FirstName.Contains(searchValue) ||
                        e.LastName.Contains(searchValue) ||
                        e.Email.Contains(searchValue) ||
                        e.Phone.Contains(searchValue) ||
                        e.Designation.Contains(searchValue) ||
                        e.Code.Contains(searchValue));
                }

                // Records count after filtering
                var filteredRecords = await employees.CountAsync();

                // Handle sorting based on column index
                if (!string.IsNullOrEmpty(sortColumnDirection))
                {
                    employees = sortColumnIndex switch
                    {
                        0 => sortColumnDirection == "asc"
                            ? employees.OrderBy(e => e.Code)
                            : employees.OrderByDescending(e => e.Code), // Code
                        1 => sortColumnDirection == "asc"
                            ? employees.OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                            : employees.OrderByDescending(e => e.FirstName).ThenByDescending(e => e.LastName), // Name
                        2 => sortColumnDirection == "asc"
                            ? employees.OrderBy(e => e.Email)
                            : employees.OrderByDescending(e => e.Email), // Email
                        3 => sortColumnDirection == "asc"
                            ? employees.OrderBy(e => e.Phone)
                            : employees.OrderByDescending(e => e.Phone), // Phone
                        4 => sortColumnDirection == "asc"
                            ? employees.OrderBy(e => e.Designation)
                            : employees.OrderByDescending(e => e.Designation), // Designation
                        5 => sortColumnDirection == "asc"
                            ? employees.OrderBy(e => e.HireDate)
                            : employees.OrderByDescending(e => e.HireDate), // Hire Date
                        6 => sortColumnDirection == "asc"
                            ? employees.OrderBy(e => e.BaseSalary)
                            : employees.OrderByDescending(e => e.BaseSalary), // Salary
                        7 => sortColumnDirection == "asc"
                            ? employees.OrderBy(e => e.IsActive)
                            : employees.OrderByDescending(e => e.IsActive), // Status
                        _ => employees.OrderBy(e => e.Id) // Default sort
                    };
                }
                else
                {
                    employees = employees.OrderBy(e => e.Id);
                }

                // Pagination
                var employeeData = await employees
                    .Skip(start)
                    .Take(length)
                    .Select(e => new
                    {
                        id = e.Id,
                        code = e.Code,
                        firstName = e.FirstName,
                        lastName = e.LastName,
                        email = e.Email,
                        phoneNumber = e.Phone,
                        designation = e.Designation,
                        hireDate = e.HireDate.ToString("yyyy-MM-dd"),
                        baseSalary = e.BaseSalary,
                        isActive = e.IsActive
                    })
                    .ToListAsync();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = filteredRecords,
                    data = employeeData
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetAllEmployees: {ex.Message}");
                return Json(new
                {
                    draw = HttpContext.Request.Query["draw"].FirstOrDefault(),
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new object[] { }
                });
            }
        } // GET: Employees/GetDeletedEmployees (Deactivated employees only)
        public async Task<IActionResult> GetDeletedEmployees()
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            // Only get deactivated employees
            var employees = _context.Employees.Where(e => !e.IsActive).AsQueryable();

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
                // Default sort by Id descending (newest deactivated first)
                employees = employees.OrderByDescending(e => e.Id);
            }

            var recordsTotal = await employees.CountAsync();
            var data = await employees.Skip(start).Take(length).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: Employees/Upsert
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            Employee employee;

            if (id == null)
            {
                // Handle create: show a blank form for new employee
                employee = new Employee();
            }
            else
            {
                // Handle update: load employee from DB
                employee = await _context.Employees
                    .Include(e => e.PaymentMethod)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (employee == null)
                    return NotFound();
            }

            var paymentMethods = await _context.PaymentMethods.ToListAsync();

            var vm = new EmployeeUpsertViewModel
            {
                Employee = employee,
                PaymentMethods = paymentMethods
            };
            return View(vm);
        }



        // POST: Employees/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(EmployeeUpsertViewModel model)
        {
            var employee = model.Employee;
            ModelState.Remove("PaymentMehtods");

            if (ModelState.IsValid)
            {
                if (employee.Id == 0)
                {
                    // Duplicate checks
                    if (await _context.Employees.AnyAsync(e => e.Code == employee.Code))
                    {
                        ModelState.AddModelError("Employee.Code", "Employee Code already exists.");
                        model.PaymentMethods = await _context.PaymentMethods.ToListAsync();
                        return View(model);
                    }
                    if (await _context.Employees.AnyAsync(e => e.Email == employee.Email))
                    {
                        ModelState.AddModelError("Employee.Email", "Email address already exists.");
                        model.PaymentMethods = await _context.PaymentMethods.ToListAsync();
                        return View(model);
                    }
                    employee.IsActive = true;
                    _context.Employees.Add(employee);
                    TempData["success"] = "Employee created successfully!";
                }
                else
                {
                    // Duplicate checks, excluding current ID
                    if (await _context.Employees.AnyAsync(e => e.Code == employee.Code && e.Id != employee.Id))
                    {
                        ModelState.AddModelError("Employee.Code", "Employee Code already exists.");
                        model.PaymentMethods = await _context.PaymentMethods.ToListAsync();
                        return View(model);
                    }
                    if (await _context.Employees.AnyAsync(e => e.Email == employee.Email && e.Id != employee.Id))
                    {
                        ModelState.AddModelError("Employee.Email", "Email address already exists.");
                        model.PaymentMethods = await _context.PaymentMethods.ToListAsync();
                        return View(model);
                    }
                    _context.Employees.Update(employee);
                    TempData["success"] = "Employee updated successfully!";
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // On invalid, re-add payment methods to model
            model.PaymentMethods = await _context.PaymentMethods.ToListAsync();
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InActive(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    return Json(new { status = false, message = "Employee not found." });
                }

                // Soft delete: Set IsActive to false
                employee.IsActive = false;
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    status = true,
                    message = $"{employee.FirstName} {employee.LastName} has been deactivated successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message = $"An error occurred while deactivating the employee: {ex.Message}"
                });
            }
        }

        // POST: Employees/Delete/5 (Soft Delete - Deactivate)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    return Json(new { status = false, message = "Employee not found." });
                }

                // Soft delete: Set IsActive to false
                employee.IsActive = false;
                employee.IsDeleted = true;
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    status = true,
                    message = $"{employee.FirstName} {employee.LastName} has been deactivated successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message = $"An error occurred while deactivating the employee: {ex.Message}"
                });
            }
        }

        // POST: Employees/Restore/5 (Reactivate)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    return Json(new { status = false, message = "Employee not found." });
                }

                // Restore: Set IsActive to true
                employee.IsActive = true;
                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    status = true,
                    message = $"{employee.FirstName} {employee.LastName} has been restored successfully."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message = $"An error occurred while restoring the employee: {ex.Message}"
                });
            }
        }

        // POST: Employees/PermanentDelete/5 (Physical Delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PermanentDelete(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    return Json(new { status = false, message = "Employee not found." });
                }

                // Check if employee has any related records that would prevent deletion
                bool hasAttendanceRecords = await _context.AttendanceRecords.AnyAsync(ar => ar.EmployeeId == id);
                bool hasPerformanceReviews = await _context.PerformanceReviews.AnyAsync(pr => pr.EmployeeId == id);
                bool hasLeaveRequests = await _context.LeaveRequests.AnyAsync(lr => lr.EmployeeId == id);
                bool hasPayrollItems = await _context.PayrollItems.AnyAsync(pi => pi.EmployeeId == id);

                if (hasAttendanceRecords || hasPerformanceReviews || hasLeaveRequests || hasPayrollItems)
                {
                    var relatedRecords = new List<string>();
                    if (hasAttendanceRecords) relatedRecords.Add("attendance records");
                    if (hasPerformanceReviews) relatedRecords.Add("performance reviews");
                    if (hasLeaveRequests) relatedRecords.Add("leave requests");
                    if (hasPayrollItems) relatedRecords.Add("payroll items");

                    return Json(new
                    {
                        status = false,
                        message = $"Cannot permanently delete employee because they have related {string.Join(", ", relatedRecords)} in the system. Please deactivate instead."
                    });
                }

                // Permanent delete (only if no related records)
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    status = true,
                    message = $"{employee.FirstName} {employee.LastName} has been permanently deleted from the system."
                });
            }
            catch (DbUpdateException ex)
            {
                // Catch database constraint violations
                return Json(new
                {
                    status = false,
                    message = "Cannot delete employee because they have related records in the system that prevent deletion. Please deactivate instead."
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    message = $"An unexpected error occurred: {ex.Message}"
                });
            }
        }

        // GET: Employees/CheckEmployeeCode (for client-side validation)
        public async Task<IActionResult> CheckEmployeeCode(string code, int id = 0)
        {
            var exists = await _context.Employees.AnyAsync(e => e.Code == code && e.Id != id);
            return Json(new { exists = exists });
        }

        // GET: Employees/CheckEmployeeEmail (for client-side validation)
        public async Task<IActionResult> CheckEmployeeEmail(string email, int id = 0)
        {
            var exists = await _context.Employees.AnyAsync(e => e.Email == email && e.Id != id);
            return Json(new { exists = exists });
        }

        // GET: Employees/GetEmployeeStats (for dashboard if needed)
        public async Task<IActionResult> GetEmployeeStats()
        {
            var totalEmployees = await _context.Employees.CountAsync();
            var activeEmployees = await _context.Employees.CountAsync(e => e.IsActive);
            var deactivatedEmployees = await _context.Employees.CountAsync(e => !e.IsActive);

            return Json(new
            {
                total = totalEmployees,
                active = activeEmployees,
                deactivated = deactivatedEmployees
            });
        }
    }
}