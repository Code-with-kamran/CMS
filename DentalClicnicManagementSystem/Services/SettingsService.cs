//using Microsoft.Extensions.Caching.Memory;
//using CMS.Data;
//using CMS.Models;
//using Microsoft.EntityFrameworkCore;

//namespace CMS.Services
//{
//    public class SettingsService : ISettingsService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IMemoryCache _cache;
//        private const string CACHE_KEY = "DefaultSettings";

//        public SettingsService(ApplicationDbContext context, IMemoryCache cache)
//        {
//            _context = context;
//            _cache = cache;
//        }

//        public async Task<DefaultSettings> GetSettingsAsync()
//        {
//            if (_cache.TryGetValue(CACHE_KEY, out DefaultSettings cachedSettings))
//                return cachedSettings;

//            var settings = await _context.DefaultSettings.FirstOrDefaultAsync() ?? new DefaultSettings
//            {
//                DefaultCurrency = "USD",
//                DefaultPaymentMethod = "Cash",
//                WorkingHours = "09:00-17:00"
//            };

//            _cache.Set(CACHE_KEY, settings, TimeSpan.FromMinutes(30));
//            return settings;
//        }

//        public async Task UpdateSettingsAsync(DefaultSettings model)
//        {
//            var settings = await _context.DefaultSettings.FirstOrDefaultAsync();
//            if (settings != null)
//            {
//                settings.DefaultCurrency = model.DefaultCurrency;
//                settings.DefaultPaymentMethod = model.DefaultPaymentMethod;
//                settings.WorkingHours = model.WorkingHours;

//                await _context.SaveChangesAsync();

//                // 4. IMPORTANT: Invalidate the currency cache
//                _cache.Remove("CurrentCurrency");
//            }
//        }

//        public DefaultSettings GetCachedSettings()
//        {
//            return _cache.TryGetValue(CACHE_KEY, out DefaultSettings settings)
//                ? settings
//                : new DefaultSettings();
//        }

//    }
//}

using CMS.Data;
using CMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CMS.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public SettingsService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<DefaultSettings> GetSettingsAsync()
        {
            // This method fetches the current settings.
            return await _context.DefaultSettings.FirstOrDefaultAsync() ?? new DefaultSettings();
        }

        public async Task UpdateSettingsAsync(DefaultSettings model)
        {
            // Find the existing settings record in the database.
            var settings = await _context.DefaultSettings.FirstOrDefaultAsync();

            if (settings != null)
            {
                // Update the properties with the new values from the form.
                settings.DefaultCurrency = model.DefaultCurrency;
                settings.DefaultPaymentMethod = model.DefaultPaymentMethod;
                settings.WorkingHours = model.WorkingHours;

                // *** THIS IS THE CRITICAL FIX ***
                // Save the changes to the database.
                await _context.SaveChangesAsync();

                // Invalidate the cache so the next request gets the new value.
                _cache.Remove("CurrentCurrency");
            }
            else
            {
                // If no settings exist, create a new record. This is good practice.
                _context.DefaultSettings.Add(model);
                await _context.SaveChangesAsync();
            }
        }
    }

    
}


