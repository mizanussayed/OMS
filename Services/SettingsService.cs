using System.Text.Json;
using OMS.Models;

namespace OMS.Services;

public interface ISettingsService
{
    Task<AppSettings> GetSettingsAsync();
    Task SaveSettingsAsync(AppSettings settings);
    Task<string> GetClothCodePrefixAsync();
    Task<string> GetOrderCodePrefixAsync();
    Task SetClothCodePrefixAsync(string prefix);
    Task SetOrderCodePrefixAsync(string prefix);
}

public class SettingsService : ISettingsService
{
    private const string SettingsFileName = "appsettings.json";
    private AppSettings? _cachedSettings;

    public async Task<AppSettings> GetSettingsAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, SettingsFileName);
            
            if (File.Exists(path))
            {
                var json = await File.ReadAllTextAsync(path);
                _cachedSettings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            else
            {
                _cachedSettings = new AppSettings();
                await SaveSettingsAsync(_cachedSettings);
            }
        }
        catch
        {
            _cachedSettings = new AppSettings();
        }

        return _cachedSettings;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, SettingsFileName);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json);
            _cachedSettings = settings;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    public async Task<string> GetClothCodePrefixAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.ClothCodePrefix;
    }

    public async Task<string> GetOrderCodePrefixAsync()
    {
        var settings = await GetSettingsAsync();
        return settings.OrderCodePrefix;
    }

    public async Task SetClothCodePrefixAsync(string prefix)
    {
        var settings = await GetSettingsAsync();
        settings.ClothCodePrefix = prefix;
        await SaveSettingsAsync(settings);
    }

    public async Task SetOrderCodePrefixAsync(string prefix)
    {
        var settings = await GetSettingsAsync();
        settings.OrderCodePrefix = prefix;
        await SaveSettingsAsync(settings);
    }
}
