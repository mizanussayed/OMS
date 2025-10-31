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
    private readonly IAlert _alertService;

    public DashboardViewModel(IDataService dataService, IAlert alertService)
    {
        _dataService = dataService;
        _alertService = alertService;
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

        if (App.CurrentUser != null)
        {
            UserRoleDescription = App.CurrentUser.Role == UserRole.Admin 
                ? "Admin Dashboard" 
                : $"Welcome, {App.CurrentUser.Name}";
        }
    }

    [RelayCommand]
    private Task Logout()
    {
        App.CurrentUser = null;

        if (App.Current?.Windows.Count > 0)
        {
            var loginPage = new LoginPage(new LoginViewModel(_dataService, _alertService));
            App.Current.Windows[0].Page = new NavigationPage(loginPage);
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task AddMaker()
    {
        var viewModel = new AddMakerViewModel(_dataService, _alertService);
        var dialog = new AddMakerDialog(viewModel);
        await Shell.Current.Navigation.PushModalAsync(dialog);
        
        // Reload data when dialog closes
        await Task.Delay(500);
        LoadData();
    }
}

public class ClothViewModel(Cloth cloth) : ObservableObject
{
    public string Id => cloth.Id.ToString();
    public string Name => cloth.Name;
    public string Color => cloth.Color;
    public double PricePerMeter => cloth.PricePerMeter;
    public double TotalMeters => cloth.TotalMeters;
    public double RemainingMeters => cloth.RemainingMeters;
    public DateTime AddedDate => cloth.AddedDate;
    public double UsagePercent => ((TotalMeters - RemainingMeters) / TotalMeters * 100);
    public double ProgressWidth => UsagePercent * 3;
}