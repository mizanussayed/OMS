using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    public DashboardViewModel(IDataService dataService)
    {
        _dataService = dataService;
        LoadData();
    }

    [ObservableProperty]
    private ObservableCollection<ClothViewModel> cloths = new();

    [ObservableProperty]
    private int totalCloths;

    [ObservableProperty]
    private int pendingOrders;

    [ObservableProperty]
    private double inventoryValue;

    [ObservableProperty]
    private ObservableCollection<Cloth> lowStockCloths = new();

    [ObservableProperty]
    private bool hasLowStockItems;

    [ObservableProperty]
    private string userRoleDescription = "Admin Dashboard";

    private async void LoadData()
    {
        var clothsList = await _dataService.GetClothsAsync();
        
        // Convert to ClothViewModel with additional UI properties
        var clothViewModels = clothsList.Select(c => new ClothViewModel(c)).ToList();
        Cloths = new ObservableCollection<ClothViewModel>(clothViewModels);
        
        TotalCloths = clothsList.Count;
        InventoryValue = clothsList.Sum(c => c.RemainingMeters * c.PricePerMeter);

        var orders = await _dataService.GetOrdersAsync();
        PendingOrders = orders.Count(o => o.Status == DressOrderStatus.Pending);

        LowStockCloths = new ObservableCollection<Cloth>(
            clothsList.Where(c => c.RemainingMeters < c.TotalMeters * 0.2)
        );
        HasLowStockItems = LowStockCloths.Any();

        // Update user role description
        if (App.CurrentUser != null)
        {
            UserRoleDescription = App.CurrentUser.Role == UserRole.Admin 
                ? "Admin Dashboard" 
                : $"Welcome, {App.CurrentUser.Name}";
        }
    }

    [RelayCommand]
    private async Task Logout()
    {
        App.CurrentUser = null;

        // Switch back to Login page
        if (App.Current?.Windows.Count > 0)
        {
            var loginPage = new LoginPage(new LoginViewModel(_dataService));
            App.Current.Windows[0].Page = new NavigationPage(loginPage);
        }
    }

    [RelayCommand]
    private async Task AddMaker()
    {
        await Shell.Current.GoToAsync("AddMaker");
    }
}

public class ClothViewModel : ObservableObject
{
    private readonly Cloth _cloth;

    public ClothViewModel(Cloth cloth)
    {
        _cloth = cloth;
    }

    public string Id => _cloth.Id.ToString();
    public string Name => _cloth.Name;
    public string Color => _cloth.Color;
    public double PricePerMeter => _cloth.PricePerMeter;
    public double TotalMeters => _cloth.TotalMeters;
    public double RemainingMeters => _cloth.RemainingMeters;
    public DateTime AddedDate => _cloth.AddedDate;

    public double UsagePercent => ((TotalMeters - RemainingMeters) / TotalMeters * 100);
    public double ProgressWidth => UsagePercent * 3; // Multiply for visual width (adjust as needed)
}