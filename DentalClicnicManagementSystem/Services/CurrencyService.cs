using CMS.Data;
using CMS.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CMS.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISettingsService _settingsService;
        private readonly IMemoryCache _cache;

        public CurrencyService(ApplicationDbContext context, ISettingsService settingsService, IMemoryCache cache)
        {
            _context = context;
            _settingsService = settingsService;
            _cache = cache;
        }

        public async Task<string> GetCurrentCurrencySymbolAsync()
        {
            var currency = await GetCurrentCurrencyAsync();
            return currency?.Symbol ?? "$";
        }

        public async Task<Currency> GetCurrentCurrencyAsync()
        {
            var cacheKey = "CurrentCurrency";
            if (_cache.TryGetValue(cacheKey, out Currency cachedCurrency))
                return cachedCurrency;

            var settings = await _settingsService.GetSettingsAsync();
            var currency = await _context.Currencies
                .FirstOrDefaultAsync(c => c.Name == settings.DefaultCurrency && c.IsActive);

            if (currency != null)
            {
                _cache.Set(cacheKey, currency, TimeSpan.FromMinutes(30));
            }

            return currency ?? new Currency { Symbol = "$", Name = "USD", Code = "USD" };
        }

        public string FormatAmount(decimal amount, string currencySymbol = null)
        {
            currencySymbol ??= "$";
            return $"{currencySymbol}{amount:N2}";
        }
    }
}
