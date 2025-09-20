using CMS.Models;
namespace CMS.ViewModels
{
    public class SettingsViewModel
    {
        
            public DefaultSettings Settings { get; set; } = new();
            public List<Currency> Currencies { get; set; } = new();
            public List<PaymentMethod> PaymentMethods { get; set; } = new();
    }

}
