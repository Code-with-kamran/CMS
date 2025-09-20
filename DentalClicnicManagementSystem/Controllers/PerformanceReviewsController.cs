// File: Controllers/PerformanceReviewsController.cs
using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Controllers
{
    //[Authorize(Roles = "HR,Admin")]
    public class PerformanceReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PerformanceReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PerformanceReviews
        public IActionResult Index()
        {
            return View();
        }

        // GET: PerformanceReviews/GetPerformanceReviews
        public async Task<IActionResult> GetPerformanceReviews()
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = int.Parse(HttpContext.Request.Query["start"].ToString());
            var length = int.Parse(HttpContext.Request.Query["length"].ToString());
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();
            var sortColumnIndex = int.Parse(HttpContext.Request.Query["order[0][column]"].ToString());
            var sortColumnDir = HttpContext.Request.Query["order[0][dir]"].ToString();

            var reviews = _context.PerformanceReviews
                                  .Include(pr => pr.Employee)
                                  .AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(searchValue))
            {
                reviews = reviews.Where(pr =>
                    pr.Employee.FirstName.Contains(searchValue) ||
                    pr.Employee.LastName.Contains(searchValue) ||
                    pr.Employee.Code.Contains(searchValue) ||
                    pr.Reviewer.Contains(searchValue) ||
                    pr.Notes.Contains(searchValue) ||
                    pr.Rating.ToString().Contains(searchValue));
            }

            // Sorting
            var sortColumn = HttpContext.Request.Query[$"columns[{sortColumnIndex}][data]"].ToString();
            if (!string.IsNullOrEmpty(sortColumn))
            {
                if (sortColumn == "employeeName")
                {
                    reviews = sortColumnDir == "asc" ?
                        reviews.OrderBy(pr => pr.Employee.FirstName).ThenBy(pr => pr.Employee.LastName) :
                        reviews.OrderByDescending(pr => pr.Employee.FirstName).ThenByDescending(pr => pr.Employee.LastName);
                }
                else if (sortColumn == "employeeCode")
                {
                    reviews = sortColumnDir == "asc" ?
                        reviews.OrderBy(pr => pr.Employee.Code) :
                        reviews.OrderByDescending(pr => pr.Employee.Code);
                }
                else
                {
                    var propertyName = sortColumn switch
                    {
                        "reviewDate" => "ReviewDate",
                        "reviewer" => "Reviewer",
                        "rating" => "Rating",
                        "notes" => "Notes",
                        _ => null
                    };

                    if (propertyName != null)
                    {
                        reviews = sortColumnDir == "asc" ?
                            reviews.OrderBy(pr => EF.Property<object>(pr, propertyName)) :
                            reviews.OrderByDescending(pr => EF.Property<object>(pr, propertyName));
                    }
                    else
                    {
                        reviews = reviews.OrderByDescending(pr => pr.ReviewDate); // Fallback
                    }
                }
            }
            else
            {
                reviews = reviews.OrderByDescending(pr => pr.ReviewDate); // Default sort
            }

            var recordsTotal = await reviews.CountAsync();
            var data = await reviews.Skip(start).Take(length).Select(pr => new
            {
                pr.Id,
                EmployeeId = pr.Employee.Id,
                EmployeeName = pr.Employee.FullName,
                EmployeeCode = pr.Employee.Code,
                ReviewDate = pr.ReviewDate.ToString("yyyy-MM-dd"),
                pr.Reviewer,
                pr.Rating,
                pr.Notes
            }).ToListAsync();

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
        }

        // GET: PerformanceReviews/Upsert (Modal content)
        public async Task<IActionResult> Upsert(int? id)
        {
            PerformanceReview review = new PerformanceReview();
            if (id != null)
            {
                review = await _context.PerformanceReviews.FindAsync(id);
                if (review == null)
                {
                    return NotFound();
                }
            }

            ViewData["EmployeeList"] = new SelectList(await _context.Employees.Where(e => e.IsActive).ToListAsync(), "Id", "FullName", review.EmployeeId);
            return PartialView("_UpsertPartial", review);
        }

        // POST: PerformanceReviews/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(PerformanceReview review)
        {
            if (ModelState.IsValid)
            {
                if (review.Id == 0)
                {
                    _context.PerformanceReviews.Add(review);
                }
                else
                {
                    _context.PerformanceReviews.Update(review);
                }
                await _context.SaveChangesAsync();
                return Json(new { status = true, message = "Performance review saved successfully!" });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { status = false, message = "Validation failed: " + string.Join("; ", errors) });
        }

        // POST: PerformanceReviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.PerformanceReviews.FindAsync(id);
            if (review == null)
            {
                return Json(new { status = false, message = "Performance review not found." });
            }

            _context.PerformanceReviews.Remove(review);
            await _context.SaveChangesAsync();

            return Json(new { status = true, message = "Performance review deleted successfully." });
        }
    }
}

