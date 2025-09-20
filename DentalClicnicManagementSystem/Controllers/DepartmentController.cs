// File: Controllers/DepartmentsController.cs
using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Web.Controllers
{
    //[Authorize(Roles = "HR,Admin")]
    public class DepartmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DepartmentsController(ApplicationDbContext db) => _db = db;

        public IActionResult Index() => View();
        [HttpGet]
        public async Task<IActionResult> List() =>
            Json(new { data = await _db.Departments.OrderBy(x => x.DepartmentName).ToListAsync() });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Department model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (model.DepartmentId == 0) _db.Add(model); else _db.Update(model);
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var d = await _db.Departments.FindAsync(id);
            if (d == null) return NotFound();
            _db.Remove(d);
            await _db.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}
