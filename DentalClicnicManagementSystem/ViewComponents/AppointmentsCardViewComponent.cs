using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CMS.Data; // Your DbContext namespace

public class AppointmentsCardViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    // Constructor to inject ApplicationDbContext
    public AppointmentsCardViewComponent(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context)); // Ensure _context is injected
    }

    // This method handles the logic for fetching and displaying appointments for the selected date
    public async Task<IViewComponentResult> InvokeAsync(DateTime? selectedDate)
    {
        // If no date is provided, default to today
        var dateToQuery = selectedDate?.Date ?? DateTime.Today;

        // Query the appointments for the selected date
        var model = await _context.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .Where(a => a.AppointmentDate.Date == dateToQuery)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        // Pass the selected date to the view for highlighting in a calendar or other UI
        ViewData["SelectedDate"] = dateToQuery;

        return View(model); // Return the model to the view for rendering
    }
}
