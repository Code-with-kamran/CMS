using CMS.Data;
using CMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class CustomHolidaysController : Controller
{
    private readonly ApplicationDbContext _ctx;
    public CustomHolidaysController(ApplicationDbContext ctx) => _ctx = ctx;

    // GET: /CustomHolidays
    public async Task<IActionResult> Index()
    {
        var holidays = await _ctx.CustomHolidays
            .OrderByDescending(x => x.Date)
            .ToListAsync();
        return View(holidays);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CustomHoliday model)
    {
        if (!ModelState.IsValid) return BadRequest("Please fill all required fields correctly.");

        _ctx.CustomHolidays.Add(model);
        await _ctx.SaveChangesAsync();

        return Ok(new
        {
            message = "Holiday added successfully.",
            id = model.Id,
            name = model.Name,
            reason = model.Reason,
            isActive = model.IsActive,
            dateIso = model.Date?.ToString("yyyy-MM-dd"),
            dateDisplay = model.Date?.ToString("dd MMM yyyy")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] CustomHoliday model)
    {
        if (!ModelState.IsValid) return BadRequest("Please fill all required fields correctly.");

        var holiday = await _ctx.CustomHolidays.FindAsync(model.Id);
        if (holiday == null) return NotFound("Record not found.");

        holiday.Name = model.Name;
        holiday.Date = model.Date;
        holiday.Reason = model.Reason;
        holiday.IsActive = model.IsActive;

        await _ctx.SaveChangesAsync();

        return Ok(new
        {
            message = "Holiday updated successfully.",
            id = holiday.Id,
            name = holiday.Name,
            reason = holiday.Reason,
            isActive = holiday.IsActive,
            dateIso = holiday.Date?.ToString("yyyy-MM-dd"),
            dateDisplay = holiday.Date?.ToString("dd MMM yyyy")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var holiday = await _ctx.CustomHolidays.FindAsync(id);
        if (holiday == null) return NotFound("Record not found.");

        _ctx.CustomHolidays.Remove(holiday);
        await _ctx.SaveChangesAsync();

        return Ok(new { message = "Holiday deleted successfully.", id });
    }
}
