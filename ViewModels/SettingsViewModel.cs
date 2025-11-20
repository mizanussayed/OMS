using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Services;

namespace OMS.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IAlert _alertService;
    
    [ObservableProperty]
    private string clothPrefix = "CLT";
    
    [ObservableProperty]
    private string orderPrefix = "ORD";
    
    [ObservableProperty]
    private string clothPrefixError = string.Empty;
    
    [ObservableProperty]
    private string orderPrefixError = string.Empty;
    
    [ObservableProperty]
    private bool isSaving;

    public SettingsViewModel(ISettingsService settingsService, IAlert alertService)
    {
        _settingsService = settingsService;
        _alertService = alertService;
        _ = LoadSettings();
    }
    
    [RelayCommand]
    private async Task LoadSettings()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            ClothPrefix = settings.ClothCodePrefix;
            OrderPrefix = settings.OrderCodePrefix;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task SaveSettings()
    {
        // Validate
        var isValid = true;
        
        if (string.IsNullOrWhiteSpace(ClothPrefix))
        {
            ClothPrefixError = "Cloth prefix is required";
            isValid = false;
        }
        else if (ClothPrefix.Length > 10)
        {
            ClothPrefixError = "Prefix must be 10 characters or less";
            isValid = false;
        }
        else
        {
            ClothPrefixError = string.Empty;
        }
        
        if (string.IsNullOrWhiteSpace(OrderPrefix))
        {
            OrderPrefixError = "Order prefix is required";
            isValid = false;
        }
        else if (OrderPrefix.Length > 10)
        {
            OrderPrefixError = "Prefix must be 10 characters or less";
            isValid = false;
        }
        else
        {
            OrderPrefixError = string.Empty;
        }
        
        if (!isValid) return;
        
        IsSaving = true;
        
        try
        {
            await _settingsService.SetClothCodePrefixAsync(ClothPrefix.ToUpper().Trim());
            await _settingsService.SetOrderCodePrefixAsync(OrderPrefix.ToUpper().Trim());
            
            await _alertService.DisplayAlert("Success", "Settings saved successfully!", "OK");
        }
        catch (Exception ex)
        {
            await _alertService.DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
        }
        finally
        {
            IsSaving = false;
        }
    }
    
    [RelayCommand]
    private async Task ResetToDefaults()
    {
        var confirmed = await _alertService.DisplayConfirmAlert(
            "Reset Settings",
            "Are you sure you want to reset to default prefixes (CLT and ORD)?",
            "Reset",
            "Cancel");
        
        if (confirmed)
        {
            ClothPrefix = "CLT";
            OrderPrefix = "ORD";
            await SaveSettings();
        }
    }
    
    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}