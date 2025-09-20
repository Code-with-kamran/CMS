using CMS.Models;

namespace CMS.ViewModels
{
    public class DefaultSettingsDisplayModel
    {
        public DefaultSettings Settings { get; set; } = new();
        public string DisplayType { get; set; } = "full";
    }
}
