using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class DashboardViewModel(IDataService dataService, IAlert alertService) : ObservableObject
{
    [ObservableProperty]
    private int totalCloths;

    [ObservableProperty]
    private int pendingOrders;

    [ObservableProperty]
    private double inventoryValue;
    
    [ObservableProperty]
    private int totalEmployees;

    [ObservableProperty]
    private ObservableCollection<ClothViewModel> lowStockCloths = [];

    [ObservableProperty]
    private bool hasLowStockItems;
    
    [ObservableProperty]
    private ObservableCollection<ClothViewModel> latestCloths = [];
    
    [ObservableProperty]
    private bool hasLatestCloths;

    [ObservableProperty]
    private string userRoleDescription = "Admin Dashboard";

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        try
        {
            // Get statistics
            var allCloths = await dataService.GetClothsAsync();
            TotalCloths = allCloths?.Count ?? 0;
            InventoryValue = allCloths?.Sum(c => c.RemainingMeters * c.PricePerMeter) ?? 0;

            var orders = await dataService.GetOrdersAsync();
            PendingOrders = orders?.Count(o => o.Status == DressOrderStatus.Pending) ?? 0;
            
            var employees = await dataService.GetEmployeesAsync();
            TotalEmployees = employees?.Count ?? 0;

            // Get low stock cloths (top 5)
            var lowStock = await dataService.GetLowStockClothsAsync(5);
            LowStockCloths = new ObservableCollection<ClothViewModel>(
                lowStock.Select(c => new ClothViewModel(c))
            );
            HasLowStockItems = LowStockCloths.Any();

            // Get latest cloths (top 5)
            var latest = await dataService.GetLatestClothsAsync(5);
            LatestCloths = new ObservableCollection<ClothViewModel>(
                latest.Select(c => new ClothViewModel(c))
            );
            HasLatestCloths = LatestCloths.Any();

            if (App.CurrentUser != null)
            {
                UserRoleDescription = App.CurrentUser.Role == UserRole.Admin 
                    ? "Admin Dashboard" 
                    : $"Welcome, {App.CurrentUser.Name}";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Dashboard LoadDataAsync error: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Initialize with empty data on error
            TotalCloths = 0;
            InventoryValue = 0;
            PendingOrders = 0;
            TotalEmployees = 0;
            HasLowStockItems = false;
            HasLatestCloths = false;
        }
    }

    [RelayCommand]
    private Task Logout()
    {
        App.CurrentUser = null;

        if (App.Current?.Windows.Count > 0)
        {
            var loginPage = new LoginPage(new LoginViewModel(dataService, alertService));
            App.Current.Windows[0].Page = new NavigationPage(loginPage);
        }

        return Task.CompletedTask;
    }

    
    [RelayCommand]
    private async Task ViewEmployees()
    {
        await Shell.Current.GoToAsync("//EmployeesPage");
    }
    
    [RelayCommand]
    private async Task OpenSettings()
    {
        var settingsService = App.Current!.Handler!.MauiContext!.Services.GetService<ISettingsService>();
        var alertService = App.Current!.Handler!.MauiContext!.Services.GetService<IAlert>();
        
        if (settingsService != null && alertService != null)
        {
            var viewModel = new SettingsViewModel(settingsService, alertService);
            var settingsPage = new SettingsPage
            {
                BindingContext = viewModel
            };
            await Shell.Current.Navigation.PushModalAsync(settingsPage);
        }
    }
}

public class ClothViewModel(Cloth cloth) : ObservableObject
{
    private readonly Cloth _cloth = cloth ?? throw new ArgumentNullException(nameof(cloth));

    public string Id => _cloth.Id.ToString();
    public string Name => _cloth!.Name;
    public string UniqueCode => _cloth.UniqueCode;
    public string DisplayName => $"{_cloth.Name} ({_cloth.UniqueCode})";
    public string Color => _cloth.Color ?? string.Empty;
    public double PricePerMeter => _cloth.PricePerMeter;
    public double TotalMeters => _cloth.TotalMeters;
    public double RemainingMeters => _cloth.RemainingMeters;
    public DateTime AddedDate => _cloth.AddedDate;
    public string AddedDateFormatted => AddedDate.ToString("MMM dd, yyyy");
    
    public double UsagePercent
    {
        get
        {
            if (TotalMeters == 0) return 0;
            return ((TotalMeters - RemainingMeters) / TotalMeters * 100);
        }
    }
    
    public double StockPercent
    {
        get
        {
            if (TotalMeters == 0) return 0;
            return (RemainingMeters / TotalMeters * 100);
        }
    }
    
    public double ProgressWidth
    {
        get
        {
            var width = UsagePercent * 3;
            return double.IsNaN(width) || double.IsInfinity(width) ? 0 : width;
        }
    }
    
    public bool IsLowStock => RemainingMeters < TotalMeters * 0.2;
    
    public Color ProgressColor
    {
        get
        {
            try
            {
                if (IsLowStock)
                    return Microsoft.Maui.Graphics.Color.FromArgb("#ea580c");
                else
                    return Microsoft.Maui.Graphics.Color.FromArgb("#9333ea");
            }
            catch
            {
                return Microsoft.Maui.Graphics.Colors.Purple;
            }
        }
    }
}