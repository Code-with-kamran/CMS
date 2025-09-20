using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMS.Controllers
{
    public class PaymentMethodController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentMethodController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var draw = Request.Query["draw"].FirstOrDefault();
                var start = Request.Query["start"].FirstOrDefault();
                var length = Request.Query["length"].FirstOrDefault();
                var searchValue = Request.Query["search[value]"].FirstOrDefault();
                var sortColumn = Request.Query["order[0][column]"].FirstOrDefault();
                var sortDirection = Request.Query["order[0][dir]"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var query = _context.PaymentMethods.AsQueryable();

                // Search
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(x => x.Name.Contains(searchValue) ||
                                           x.Description.Contains(searchValue));
                }

                // Total records before filtering
                int recordsTotal = await _context.PaymentMethods.CountAsync();
                int recordsFiltered = await query.CountAsync();

                // Sorting
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
                {
                    switch (sortColumn)
                    {
                        case "0":
                            query = sortDirection == "asc" ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                            break;
                        case "1":
                            query = sortDirection == "asc" ? query.OrderBy(x => x.Description) : query.OrderByDescending(x => x.Description);
                            break;
                        case "2":
                            query = sortDirection == "asc" ? query.OrderBy(x => x.IsActive) : query.OrderByDescending(x => x.IsActive);
                            break;
                        default:
                            query = query.OrderBy(x => x.Name);
                            break;
                    }
                }
                else
                {
                    query = query.OrderBy(x => x.Name);
                }

                // Pagination
                var data = await query.Skip(skip).Take(pageSize).ToListAsync();

                return Json(new
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods.FindAsync(id);
                if (paymentMethod == null)
                {
                    return Json(new { status = false, message = "Payment method not found." });
                }

                return Json(new { status = true, data = paymentMethod });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert([FromBody] PaymentMethod model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { status = false, message = string.Join(", ", errors) });
                }

                // Check for duplicate names (excluding current record if editing)
                var existingPaymentMethod = await _context.PaymentMethods
                    .FirstOrDefaultAsync(x => x.Name.ToLower() == model.Name.ToLower() && x.Id != model.Id);

                if (existingPaymentMethod != null)
                {
                    return Json(new { status = false, message = "A payment method with this name already exists." });
                }

                if (model.Id == 0) // Create
                {
                    // If this is set as default, unset all others
                    if (model.IsDefault)
                    {
                        var defaultPaymentMethods = await _context.PaymentMethods.Where(x => x.IsDefault).ToListAsync();
                        foreach (var pm in defaultPaymentMethods)
                        {
                            pm.IsDefault = false;
                        }
                    }

                    model.CreatedAt = DateTime.UtcNow;
                    _context.PaymentMethods.Add(model);
                    await _context.SaveChangesAsync();

                    return Json(new { status = true, message = "Payment method added successfully!" });
                }
                else // Update
                {
                    var existingRecord = await _context.PaymentMethods.FindAsync(model.Id);
                    if (existingRecord == null)
                    {
                        return Json(new { status = false, message = "Payment method not found." });
                    }

                    // If this is set as default, unset all others
                    if (model.IsDefault && !existingRecord.IsDefault)
                    {
                        var defaultPaymentMethods = await _context.PaymentMethods.Where(x => x.IsDefault && x.Id != model.Id).ToListAsync();
                        foreach (var pm in defaultPaymentMethods)
                        {
                            pm.IsDefault = false;
                        }
                    }

                    existingRecord.Name = model.Name;
                    existingRecord.Description = model.Description;
                    existingRecord.IsDefault = model.IsDefault;
                    existingRecord.IsActive = model.IsActive;
                    existingRecord.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();

                    return Json(new { status = true, message = "Payment method updated successfully!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var paymentMethod = await _context.PaymentMethods.FindAsync(id);
                if (paymentMethod == null)
                {
                    return Json(new { status = false, message = "Payment method not found." });
                }

                // Check if this is the default payment method
                if (paymentMethod.IsDefault)
                {
                    return Json(new { status = false, message = "Cannot delete the default payment method. Please set another payment method as default first." });
                }

                _context.PaymentMethods.Remove(paymentMethod);
                await _context.SaveChangesAsync();

                return Json(new { status = true, message = "Payment method deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
    }

}
