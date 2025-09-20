using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.Controllers
{
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Inventory
        public IActionResult Index()
        {
            return View();
        }

        // GET: Inventory/GetData (for DataTables AJAX)
        public async Task<IActionResult> GetData()
        {
            var draw = HttpContext.Request.Query["draw"].ToString();
            var start = HttpContext.Request.Query["start"].ToString();
            var length = HttpContext.Request.Query["length"].ToString();
            var sortColumn = HttpContext.Request.Query["columns[" + HttpContext.Request.Query["order[0][column]"].ToString() + "][name]"].ToString();
            var sortColumnDirection = HttpContext.Request.Query["order[0][dir]"].ToString();
            var searchValue = HttpContext.Request.Query["search[value]"].ToString();

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            var inventoryData = _context.InventoryItems.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchValue))
            {
                inventoryData = inventoryData.Where(m => m.Name.Contains(searchValue)
                                            || m.SKU.Contains(searchValue)
                                            || m.Category.Contains(searchValue)
                                            || m.Description.Contains(searchValue));
            }

            // Sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                // Basic sorting for common columns. Extend as needed.
                if (sortColumn == "name")
                {
                    inventoryData = sortColumnDirection == "asc" ? inventoryData.OrderBy(i => i.Name) : inventoryData.OrderByDescending(i => i.Name);
                }
                else if (sortColumn == "sku")
                {
                    inventoryData = sortColumnDirection == "asc" ? inventoryData.OrderBy(i => i.SKU) : inventoryData.OrderByDescending(i => i.SKU);
                }
                else if (sortColumn == "stock")
                {
                    inventoryData = sortColumnDirection == "asc" ? inventoryData.OrderBy(i => i.Stock) : inventoryData.OrderByDescending(i => i.Stock);
                }
                else if (sortColumn == "unitPrice")
                {
                    inventoryData = sortColumnDirection == "asc" ? inventoryData.OrderBy(i => i.UnitPrice) : inventoryData.OrderByDescending(i => i.UnitPrice);
                }
                else if (sortColumn == "category")
                {
                    inventoryData = sortColumnDirection == "asc" ? inventoryData.OrderBy(i => i.Category) : inventoryData.OrderByDescending(i => i.Category);
                }
                else
                {
                    // Default sort if column not explicitly handled
                    inventoryData = inventoryData.OrderBy(i => i.Id);
                }
            }
            else
            {
                // Default sort if no sort column is provided
                inventoryData = inventoryData.OrderBy(i => i.Id);
            }


            recordsTotal = await inventoryData.CountAsync();

            var data = await inventoryData.Skip(skip).Take(pageSize).ToListAsync();

            var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
            return Ok(jsonData);
        }

        // GET: Inventory/Upsert
        public async Task<IActionResult> Upsert(int? id)
        {
            InventoryItem inventoryItem = new InventoryItem();
            if (id == null || id == 0)
            {
                // Create
                return View(inventoryItem);
            }
            else
            {
                // Edit
                inventoryItem = await _context.InventoryItems.FindAsync(id);
                if (inventoryItem == null)
                {
                    return NotFound();
                }
                return View(inventoryItem);
            }
        }

        // POST: Inventory/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(InventoryItem inventoryItem)
        {
            if (ModelState.IsValid)
            {
                if (inventoryItem.Id == 0)
                {
                    // Create
                    _context.InventoryItems.Add(inventoryItem);
                    TempData["success"] = "Inventory item created successfully!";
                }
                else
                {
                    // Update
                    _context.InventoryItems.Update(inventoryItem);
                    TempData["success"] = "Inventory item updated successfully!";
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inventoryItem);
        }


        [HttpGet]
        public IActionResult GetActiveItems()
        {
            var items = _context.InventoryItems
                .Where(i => i.IsActive)
                .Select(i => new {
                    id = i.Id,
                    name = i.Name,
                    unitPrice = i.UnitPrice,
                    stock = i.Stock,
                    description = i.Description ?? ""
                })
                .ToList();

            return Json(items);
        }


        // POST: Inventory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(id);
            if (inventoryItem == null)
            {
                return Json(new { status = false, message = "Inventory item not found." });
            }

            _context.InventoryItems.Remove(inventoryItem);
            await _context.SaveChangesAsync();
            return Json(new { status = true, message = "Inventory item deleted successfully!" });
        }
    }
}
