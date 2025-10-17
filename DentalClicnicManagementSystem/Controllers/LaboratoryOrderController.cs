using CMS.Data;
using CMS.Models;
using CMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CMS.Controllers
{
    public class LaboratoryOrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<LaboratoryOrderController> _logger;

        public LaboratoryOrderController(ApplicationDbContext db, ILogger<LaboratoryOrderController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // GET: /LaboratoryOrder/
        public IActionResult Index()
        {
            return View();
        }

        // GET: /LaboratoryOrder/Upsert/{id?}
        [HttpGet]
        public async Task<IActionResult> Upsert(int? id)
        {
            var viewModel = new LaboratoryOrderViewModel
            {
                Patients = await _db.Patients.Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.FirstName + " " + p.LastName }).ToListAsync(),
            };

            if (id == null || id == 0)
            {
                // Create new order
                viewModel.Order = new LaboratoryOrder();
            }
            else
            {
                // Edit existing order
                viewModel.Order = await _db.LaboratoryOrders.FindAsync(id);
                if (viewModel.Order == null)
                {
                    return NotFound();
                }
            }
            return View(viewModel);
        }

        // POST: /LaboratoryOrder/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(LaboratoryOrderViewModel viewModel)
        {
            ModelState.Remove("Order.Patient"); // Remove navigation property validation
            if (ModelState.IsValid)
            {
                if (viewModel.Order.Id == 0)
                {
                    // Create
                    viewModel.Order.CreatedAt = DateTime.Now;
                    _db.LaboratoryOrders.Add(viewModel.Order);
                    TempData["success"] = "Laboratory order created successfully.";
                }
                else
                {
                    // Update
                    var orderFromDb = await _db.LaboratoryOrders.FindAsync(viewModel.Order.Id);
                    if (orderFromDb == null) return NotFound();

                    orderFromDb.TestName = viewModel.Order.TestName;
                    orderFromDb.TestCode = viewModel.Order.TestCode;
                    orderFromDb.PatientId = viewModel.Order.PatientId;
                    orderFromDb.TestPrice = viewModel.Order.TestPrice;
                    orderFromDb.Status = viewModel.Order.Status;
                    orderFromDb.OrderDate = viewModel.Order.OrderDate;
                    orderFromDb.CollectionDate = viewModel.Order.CollectionDate;
                    orderFromDb.ResultDate = viewModel.Order.ResultDate;
                    orderFromDb.Result = viewModel.Order.Result;
                    orderFromDb.Notes = viewModel.Order.Notes;
                    orderFromDb.UpdatedAt = DateTime.Now;

                    _db.LaboratoryOrders.Update(orderFromDb);
                    TempData["success"] = "Laboratory order updated successfully.";
                }

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If model state is invalid, repopulate dropdowns and return to view
            viewModel.Patients = await _db.Patients.Select(p => new SelectListItem { Value = p.PatientId.ToString(), Text = p.FirstName + " " + p.LastName }).ToListAsync();
            return View(viewModel);
        }

        // POST: /LaboratoryOrder/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var order = await _db.LaboratoryOrders.FindAsync(id);
                if (order == null)
                {
                    return Json(new { status = false, message = "Order not found." });
                }

                // Prevent deletion if an invoice is linked to this order
                bool hasInvoice = await _db.Invoices.AnyAsync(i => i.LaboratoryOrderId == id);
                if (hasInvoice)
                {
                    return Json(new { status = false, message = "Cannot delete. This order is linked to an invoice." });
                }

                _db.LaboratoryOrders.Remove(order);
                await _db.SaveChangesAsync();

                return Json(new { status = true, message = "Order deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting laboratory order with ID {OrderId}", id);
                return Json(new { status = false, message = "An error occurred while deleting the order." });
            }
        }

        #region API Calls

        // GET: /LaboratoryOrder/GetLaboratoryOrderList
        [HttpGet]
        public async Task<IActionResult> GetLaboratoryOrderList()
        {
            try
            {
                var draw = Request.Query["draw"].FirstOrDefault();
                var start = int.Parse(Request.Query["start"].FirstOrDefault() ?? "0");
                var length = int.Parse(Request.Query["length"].FirstOrDefault() ?? "10");
                var searchValue = (Request.Query["search[value]"].FirstOrDefault() ?? string.Empty).Trim().ToLower();
                var orderColIndex = int.Parse(Request.Query["order[0][column]"].FirstOrDefault() ?? "0");
                var orderDir = Request.Query["order[0][dir]"].FirstOrDefault() ?? "asc";

                IQueryable<LaboratoryOrder> query = _db.LaboratoryOrders
                    .Include(o => o.Patient)
                    .AsNoTracking();

                int recordsTotal = await query.CountAsync();

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    query = query.Where(o =>
                        o.TestName.ToLower().Contains(searchValue) ||
                        (o.TestCode != null && o.TestCode.ToLower().Contains(searchValue)) ||
                        (o.Patient.FirstName + " " + o.Patient.LastName).ToLower().Contains(searchValue) 
                    );
                }

                int recordsFiltered = await query.CountAsync();

                Expression<Func<LaboratoryOrder, object>> sortSelector = orderColIndex switch
                {
                    0 => o => o.TestName,
                    1 => o => o.Patient.FirstName,
                    3 => o => o.OrderDate,
                    4 => o => o.TestPrice,
                    5 => o => o.Status,
                    _ => o => o.CreatedAt
                };

                query = orderDir.Equals("asc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderBy(sortSelector)
                    : query.OrderByDescending(sortSelector);

                var data = await query
                    .Skip(start)
                    .Take(length)
                    .Select(o => new
                    {
                        id = o.Id,
                        testName = o.TestName,
                        patientName = o.Patient.FirstName + " " + o.Patient.LastName,
                        orderDate = o.OrderDate,
                        price = o.TestPrice,
                        status = o.Status.ToString()
                    })
                    .ToListAsync();

                return Json(new { draw, recordsTotal, recordsFiltered, data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching laboratory order list for DataTables.");
                return StatusCode(500, new { error = "Server error: " + ex.Message });
            }
        }

        #endregion
    }
}
