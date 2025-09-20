// File: ViewComponents/AllAppointmentsTableViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using CMS.Data;
namespace CMS.ViewComponents
{
    public class AllAppointmentsTableViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public AllAppointmentsTableViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5) // Show latest 5
                .ToListAsync();

            return View(model);
        }
    }
}