using CMS.Data;
using CMS.Hubs;
using CMS.Models;
using CMS.Services;
using CMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CMS.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettingsService _settingsService;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<SettingsHub> _hubContext;

        public SettingsController(ISettingsService settingsService, ApplicationDbContext context, IHubContext<SettingsHub> hubContext)
        {
            _settingsService = settingsService;
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new SettingsViewModel
            {
                Settings = await _settingsService.GetSettingsAsync(),
                Currencies = await _context.Currencies.Where(c => c.IsActive).ToListAsync(),
                PaymentMethods = await _context.PaymentMethods.Where(p => p.IsActive).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index([FromBody] DefaultSettings model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { status = false, message = string.Join(", ", errors) });
                }

                await _settingsService.UpdateSettingsAsync(model);

                // Notify all connected users about the settings change
                await _hubContext.Clients.Group("SettingsGroup").SendAsync("SettingsUpdated", new
                {
                    defaultCurrency = model.DefaultCurrency,
                    defaultPaymentMethod = model.DefaultPaymentMethod,
                    workingHours = model.WorkingHours
                });

                return Json(new { status = true, message = "Settings updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrent()
        {
            var settings = await _settingsService.GetSettingsAsync();
            return Json(new
            {
                defaultCurrency = settings.DefaultCurrency,
                defaultPaymentMethod = settings.DefaultPaymentMethod,
                workingHours = settings.WorkingHours
            });
        }
    }
}
