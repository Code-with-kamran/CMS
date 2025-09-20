// Ensure you have the interface defined
using CMS.Models;

public interface ISettingsService
{
    Task<DefaultSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(DefaultSettings model);
}