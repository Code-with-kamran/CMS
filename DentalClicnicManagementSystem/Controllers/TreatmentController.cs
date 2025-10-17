using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CMS.Controllers
{
    public class TreatmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TreatmentController> _logger;

        public TreatmentController(ApplicationDbContext context, ILogger<TreatmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Treatment
        public async Task<IActionResult> Index()
        {
            return View();
        }

        // GET: Treatment/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var treatment = await _context.Treatments.FirstOrDefaultAsync(t => t.TreatmentId == id);
            if (treatment == null)
                return NotFound();

            return View(treatment);
        }

        // GET: Treatment/Upsert or Treatment/Upsert/5
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            Treatment model;

            if (id == null || id == 0)
            {
                // New treatment
                model = new Treatment();
            }
            else
            {
                model = await _context.Treatments
                    .FirstOrDefaultAsync(x => x.TreatmentId == id.Value);

                if (model == null)
                    return NotFound();
            }

            return View(model);
        }

        // POST: Treatment/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(Treatment treatment)
        {
            var isCreate = treatment.TreatmentId == 0;

            if (!ModelState.IsValid)
                return View(treatment);

            if (isCreate)
            {
                treatment.CreatedAt = DateTime.Now;
                treatment.CreatedBy = User.Identity?.Name ?? "System";
                _context.Treatments.Add(treatment);

                await _context.SaveChangesAsync();
                TempData["success"] = "Treatment created successfully.";
            }
            else
            {
                var dbTreatment = await _context.Treatments
                    .FirstOrDefaultAsync(x => x.TreatmentId == treatment.TreatmentId);

                if (dbTreatment == null) return NotFound();

                // Update properties
                dbTreatment.Name = treatment.Name;
                dbTreatment.Description = treatment.Description;
                dbTreatment.UnitPrice = treatment.UnitPrice;
                dbTreatment.DurationMinutes = treatment.DurationMinutes;
                dbTreatment.IsActive = treatment.IsActive;
                dbTreatment.UpdatedAt = DateTime.Now;
                dbTreatment.UpdatedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();
                TempData["success"] = "Treatment updated successfully.";
            }

            return RedirectToAction("Index");
        }

        // GET: Treatment/GetTreatmentList (for DataTables)
        [HttpGet]
        public async Task<IActionResult> GetTreatmentList()
        {
            try
            {
                // DataTables params
                int draw = int.TryParse(Request.Query["draw"], out var d) ? d : 1;
                int start = int.TryParse(Request.Query["start"], out var s) ? s : 0;
                int length = int.TryParse(Request.Query["length"], out var l) ? l : 10;
                string searchValue = (Request.Query["search[value]"].FirstOrDefault() ?? string.Empty).Trim();

                int orderColIndex = int.TryParse(Request.Query["order[0][column]"], out var oc) ? oc : 0;
                string orderDir = Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";

                if (length <= 0) length = 10;
                if (start < 0) start = 0;

                IQueryable<Treatment> q = _context.Treatments.AsNoTracking();

                int recordsTotal = await q.CountAsync();

                // Search
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    string term = searchValue.ToLower();
                    q = q.Where(t =>
                        (t.Name ?? "").ToLower().Contains(term) ||
                        (t.Description ?? "").ToLower().Contains(term) ||
                        t.UnitPrice.ToString().Contains(term)
                    );
                }

                int recordsFiltered = await q.CountAsync();

                // Sort mapping
                Expression<Func<Treatment, object>> sortSelector = orderColIndex switch
                {
                    0 => t => t.Name,
                    2 => t => t.UnitPrice,
                    3 => t => t.DurationMinutes,
                    4 => t => t.CreatedAt,
                    5 => t => t.IsActive,
                    _ => t => t.CreatedAt
                };

                bool desc = orderDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
                q = desc ? q.OrderByDescending(sortSelector) : q.OrderBy(sortSelector);

                // Page + shape
                var data = await q
                    .Skip(start)
                    .Take(length)
                    .Select(t => new
                    {
                        id = t.TreatmentId,
                        name = t.Name,
                        description = t.Description,
                        price = t.UnitPrice,
                        durationMinutes = t.DurationMinutes,
                        isActive = t.IsActive,
                        createdAt = t.CreatedAt,
                        updatedAt = t.UpdatedAt,
                        createdBy = t.CreatedBy
                    })
                    .ToListAsync();

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

        // POST: Treatment/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var treatment = await _context.Treatments.FindAsync(id);
                if (treatment == null)
                {
                    return Json(new { status = false, message = "Treatment not found." });
                }

                // Check if treatment is being used in appointments or other relations
                // Add your business logic here if needed

                _context.Treatments.Remove(treatment);
                await _context.SaveChangesAsync();

                return Json(new { status = true, message = "Treatment deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting treatment with ID {TreatmentId}", id);
                return Json(new { status = false, message = "Error deleting treatment." });
            }
        }
    }
}
