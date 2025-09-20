using Microsoft.AspNetCore.Mvc;
using CMS.Services;

namespace CMS.ViewComponents
{
    public class DefaultSettingsViewComponent : ViewComponent
    {
        private readonly ISettingsService _settingsService;

        public DefaultSettingsViewComponent(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string displayType = "full")
        {
            var settings = await _settingsService.GetSettingsAsync();
            ViewData["DisplayType"] = displayType;
            return View(settings);
        }
    }
}
