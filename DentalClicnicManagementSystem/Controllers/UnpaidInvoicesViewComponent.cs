// ViewComponents/UnpaidInvoicesViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMS.Data;
using System.Linq;
using System.Threading.Tasks;

namespace CMS.ViewComponents
{
    public class UnpaidInvoicesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public UnpaidInvoicesViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var unpaidInvoices = await _context.Invoices
                .Where(i => i.Status != "Paid")
                .Select(i => new
                {
                    i.Id,
                    i.InvoiceNumber,
                    PatientName = string.Concat(i.Patient.FirstName, " ", i.Patient.LastName) ,
                    i.Total
                })
                .ToListAsync();

            ViewData["TotalUnpaid"] = unpaidInvoices.Sum(i => i.Total);
            return View(unpaidInvoices);
        }
    }
}
