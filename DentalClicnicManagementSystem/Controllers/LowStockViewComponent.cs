// ViewComponents/LowStockViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.ViewComponents
{
    public class LowStockViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public LowStockViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var lowStockItems = await _context.InventoryItems
                .Where(i => i.Stock < 5)
                .Select(i => new
                {
                    i.Id,
                    i.Name,
                    i.Stock
                })
                .ToListAsync();

            ViewData["Count"] = lowStockItems.Count;
            return View(lowStockItems);
        }
    }
}
