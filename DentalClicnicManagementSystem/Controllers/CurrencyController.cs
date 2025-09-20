using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CMS.Controllers
{
    public class CurrencyController : Controller
    {
        // Inject ApplicationDbContext into the controller
        private readonly ApplicationDbContext _context;
        public CurrencyController(ApplicationDbContext context) => _context = context;

        // GET: /Currency
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Currency/GetList
        [HttpGet]
        public async Task<IActionResult> GetList()
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

                // Base query for the Currency DbSet
                IQueryable<Currency> q = _context.Currencies.AsQueryable();

                // Search (across multiple fields)
                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    string term = searchValue.ToLower();
                    q = q.Where(c =>
                        c.Name.ToLower().Contains(term) ||
                        c.Code.ToLower().Contains(term) ||
                        c.Symbol.ToLower().Contains(term)
                    );
                }

                int recordsTotal = await q.CountAsync();  // Get total records before applying filter

                int recordsFiltered = recordsTotal;  // For now, set recordsFiltered as total before pagination/filtering

                // Sorting (map DataTable column index → field)
                Expression<Func<Currency, object>> sortSelector = orderColIndex switch
                {
                    0 => c => c.Name,
                    1 => c => c.Code,
                    2 => c => c.Symbol,
                    3 => c => c.ExchangeRate,
                    4 => c => c.IsDefault,
                    5 => c => c.IsActive,
                    _ => c => c.Name  // Default sorting by Name if the column index doesn't match
                };

                bool desc = orderDir.Equals("desc", StringComparison.OrdinalIgnoreCase);
                q = desc ? q.OrderByDescending(sortSelector) : q.OrderBy(sortSelector);

                // Paging + projection
                var data = await q
                    .Skip(start)  // Apply pagination start index
                    .Take(length) // Apply pagination length
                    .Select(c => new
                    {
                        c.Id,
                        c.Name,
                        c.Code,
                        c.Symbol,
                        c.ExchangeRate,
                        c.IsDefault,
                        c.IsActive
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

        // POST: /Currency/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert([FromBody] Currency model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { status = false, message = "Invalid data." });
            }

            if (model.Id == 0)
            {
                // Create new currency

                if (model.IsDefault)
                {
                    // Reset other defaults
                    foreach (var c in _context.Currencies)
                        c.IsDefault = false;
                }

                _context.Currencies.Add(model);
                _context.SaveChanges(); // Don't forget to save changes after adding
                return Json(new { status = true, message = "Currency added successfully." });
            }
            else
            {
                // Update existing currency
                var existing = _context.Currencies.FirstOrDefault(c => c.Id == model.Id);
                if (existing == null)
                    return Json(new { status = false, message = "Currency not found." });

                if (model.IsDefault)
                {
                    // Reset other defaults
                    foreach (var c in _context.Currencies)
                        c.IsDefault = false;
                }

                existing.Name = model.Name;
                existing.Code = model.Code;
                existing.Symbol = model.Symbol;
                existing.ExchangeRate = model.ExchangeRate;
                existing.IsDefault = model.IsDefault;
                existing.IsActive = model.IsActive;

                _context.SaveChanges(); // Save changes after update
                return Json(new { status = true, message = "Currency updated successfully." });
            }
        }

        // DELETE: /Currency/Delete/{id}
        [HttpDelete]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var currency = _context.Currencies.FirstOrDefault(c => c.Id == id);
            if (currency == null)
                return Json(new { status = false, message = "Currency not found." });

            _context.Currencies.Remove(currency);
            _context.SaveChanges(); // Save changes after deletion
            return Json(new { status = true, message = "Currency deleted successfully." });
        }

        // GET: /Currency/Get/{id}
        [HttpGet]
        public IActionResult Get(int id)
        {
            var currency = _context.Currencies.FirstOrDefault(c => c.Id == id);
            if (currency == null)
                return Json(new { status = false, message = "Currency not found." });
            return Json(new { status = true, data = currency });
        }


        // GET: /Currency/GetAllCurrencies
        // this will be use in invoices 
        [HttpGet]
        public IActionResult GetAllCurrencies()
        {
            try
            {
                // Fetch all currencies from the database
                var currencies = _context.Currencies
                    .Where(c => c.IsActive) // Optionally filter active currencies
                    .Select(c => new
                    {
                        c.Code,         // Currency Code
                        c.Name,         // Currency Name
                        c.Symbol        // Currency Symbol
                    })
                    .ToList();

                return Json(currencies); // Return as JSON
            }
            catch (Exception ex)
            {
                // Return an error response if something goes wrong
                return Json(new { error = "An error occurred while fetching currencies: " + ex.Message });
            }
        }


    }
}
