// In: ViewComponents/PopularDoctorsViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMS.Data;
using CMS.ViewModels; // Make sure this using statement is present

public class PopularDoctorsViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public PopularDoctorsViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Default to weekly view on initial load
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(6);

        var popularDoctors = await _context.Appointments
            .Where(a => a.AppointmentDate >= startOfWeek && a.AppointmentDate <= endOfWeek && a.DoctorId != null)
            .GroupBy(a => a.Doctor)
            // THE FIX: This .Select() statement was likely missing or incorrect.
            // It transforms the grouped data into the correct ViewModel.
            .Select(g => new PopularDoctorViewModel
            {
                Doctor = g.Key,
                BookingCount = g.Count()
            })
            .OrderByDescending(d => d.BookingCount)
            .Take(3)
            .ToListAsync();

        // Now, 'popularDoctors' is a List<PopularDoctorViewModel>, which matches the view's model.
        return View(popularDoctors);
    }
}
