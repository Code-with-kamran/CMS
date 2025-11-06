//// File: Controllers/DepartmentsController.cs
//using CMS.Data;
//using CMS.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace Clinic.Web.Controllers
//{
//    //[Authorize(Roles = "HR,Admin")]
//    public class DepartmentsController : Controller
//    {
//        private readonly ApplicationDbContext _db;
//        public DepartmentsController(ApplicationDbContext db) => _db = db;

//        public IActionResult Index() => View();
//        [HttpGet]
//        public async Task<IActionResult> List() =>
//            Json(new { data = await _db.Departments.OrderBy(x => x.DepartmentName).ToListAsync() });

//        [HttpPost, ValidateAntiForgeryToken]
//        public async Task<IActionResult> Upsert(Department model)
//        {
//            if (!ModelState.IsValid) return BadRequest(ModelState);
//            if (model.DepartmentId == 0) _db.Add(model); else _db.Update(model);
//            await _db.SaveChangesAsync();
//            return Ok(new { success = true });
//        }

//        [HttpPost, ValidateAntiForgeryToken]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var d = await _db.Departments.FindAsync(id);
//            if (d == null) return NotFound();
//            _db.Remove(d);
//            await _db.SaveChangesAsync();
//            return Ok(new { success = true });
//        }
//    }


//}



// File: Controllers/DepartmentsController.cs
using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Clinic.Web.Controllers
{
    //[Authorize(Roles = "HR,Admin")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DepartmentsController(ApplicationDbContext db) => _db = db;

        public IActionResult Index() => View();

        // GET: /Departments/GetList - For DataTables server-side processing
        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            try
            {
                // DataTables parameters
                int draw = int.TryParse(Request.Query["draw"], out var d) ? d : 1;
                int start = int.TryParse(Request.Query["start"], out var s) ? s : 0;
                int length = int.TryParse(Request.Query["length"], out var l) ? l : 10;
                string searchValue = (Request.Query["search[value]"].FirstOrDefault() ?? string.Empty).Trim();

                int orderColIndex = int.TryParse(Request.Query["order[0][column]"], out var oc) ? oc : 0;
                string orderDir = Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";

                if (length <= 0) length = 10;
                if (start < 0) start = 0;

                // Base query
                IQueryable<Department> q = _db.Departments.AsQueryable();

                // Search (across multiple fields)
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    string term = searchValue.ToLower();
                    q = q.Where(d =>
                        d.DepartmentName.ToLower().Contains(term) ||
                        (d.Description != null && d.Description.ToLower().Contains(term))
                    );
                }

                int recordsTotal = await q.CountAsync();

                int recordsFiltered = recordsTotal;

                // Sorting (map DataTable column index → field)
                Expression<Func<Department, object>> sortSelector = orderColIndex switch
                {
                    0 => d => d.DepartmentName,
                    1 => d => d.Description ?? "",
                    2 => d => d.IsActive,
                    _ => d => d.DepartmentName  // Default sorting by DepartmentName
                };

                bool desc = orderDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
                q = desc ? q.OrderByDescending(sortSelector) : q.OrderBy(sortSelector);

                // Paging + projection
                var data = await q
                    .Skip(start)
                    .Take(length)
                    .Select(d => new
                    {
                        d.DepartmentId,
                        d.DepartmentName,
                        d.Description,
                        d.IsActive
                    })
                    .ToListAsync();

                // Return JSON for DataTable
                return Json(new
                {
                    draw,
                    recordsTotal,
                    recordsFiltered,
                    data
                });
            }
            catch (Exception ex)
            {
                Response.StatusCode = StatusCodes.Status500InternalServerError;
                return Json(new { error = "Server error: " + ex.Message });
            }
        }

        // GET: /Departments/List - For simple list (used in modal editing)
        [HttpGet]
        public async Task<IActionResult> List() =>
            Json(new { data = await _db.Departments.OrderBy(x => x.DepartmentName).ToListAsync() });

        // GET: /Departments/Get/{id} - For editing specific department
        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var department = await _db.Departments.FindAsync(id);
            if (department == null)
                return Json(new { status = false, message = "Department not found." });
            return Json(new { status = true, data = department });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Department model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (model.DepartmentId == 0)
                {
                    _db.Departments.Add(model);
                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Department added successfully.";
                }
                else
                {
                    var existing = await _db.Departments.FindAsync(model.DepartmentId);
                    if (existing == null)
                    {
                        TempData["ErrorMessage"] = "Department not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    existing.DepartmentName = model.DepartmentName;
                    existing.Description = model.Description;
                    existing.IsActive = model.IsActive;

                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Department updated successfully.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error saving department: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var department = await _db.Departments.FindAsync(id);
                if (department == null)
                    return Json(new { success = false, message = "Department not found." });

                _db.Departments.Remove(department);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Department deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting department: " + ex.Message });
            }
        }
    }
}