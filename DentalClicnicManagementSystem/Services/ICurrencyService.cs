using CMS.Models;

namespace CMS.Services
{
    public interface ICurrencyService
    {
        Task<string> GetCurrentCurrencySymbolAsync();
        Task<Currency> GetCurrentCurrencyAsync();
        string FormatAmount(decimal amount, string currencySymbol = null);
    }
}
