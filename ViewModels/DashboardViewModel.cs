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
    private ObservableCollection<ClothViewModel> cloths = [];

    [ObservableProperty]
    private int totalCloths;

    [ObservableProperty]
    private int pendingOrders;

    [ObservableProperty]
    private double inventoryValue;

    [ObservableProperty]
    private ObservableCollection<Cloth> lowStockCloths = [];

    [ObservableProperty]
    private bool hasLowStockItems;

    [ObservableProperty]
    private string userRoleDescription = "Admin Dashboard";

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        try
        {
            var clothsList = await dataService.GetClothsAsync();
            
            if (clothsList == null || clothsList.Count == 0)
            {
                Cloths = new ObservableCollection<ClothViewModel>();
                TotalCloths = 0;
                InventoryValue = 0;
                PendingOrders = 0;
                HasLowStockItems = false;
                return;
            }
            
            var clothViewModels = clothsList
                .Where(c => c != null)
                .Select(c => new ClothViewModel(c))
                .ToList();

            Cloths = new ObservableCollection<ClothViewModel>(clothViewModels);
            
            TotalCloths = clothsList.Count;
            InventoryValue = clothsList.Sum(c => c.RemainingMeters * c.PricePerMeter);

            var orders = await dataService.GetOrdersAsync();
            PendingOrders = orders?.Count(o => o.Status == DressOrderStatus.Pending) ?? 0;

            LowStockCloths = new ObservableCollection<Cloth>(
                clothsList.Where(c => c != null && c.RemainingMeters < c.TotalMeters * 0.2)
            );
            HasLowStockItems = LowStockCloths.Any();

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
            Cloths = new ObservableCollection<ClothViewModel>();
            TotalCloths = 0;
            InventoryValue = 0;
            PendingOrders = 0;
            HasLowStockItems = false;
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
    private async Task AddMaker()
    {
        var viewModel = new AddMakerViewModel(dataService, alertService);
        var dialog = new AddMakerDialog(viewModel);
        await Shell.Current.Navigation.PushModalAsync(dialog);
        
        await LoadDataAsync();
    }
}

public class ClothViewModel : ObservableObject
{
    private readonly Cloth _cloth;

    public ClothViewModel(Cloth cloth)
    {
        _cloth = cloth ?? throw new ArgumentNullException(nameof(cloth));
    }

    public string Id => _cloth.Id.ToString();
    public string Name => _cloth.Name ?? string.Empty;
    public string Color => _cloth.Color ?? string.Empty;
    public double PricePerMeter => _cloth.PricePerMeter;
    public double TotalMeters => _cloth.TotalMeters;
    public double RemainingMeters => _cloth.RemainingMeters;
    public DateTime AddedDate => _cloth.AddedDate;
    
    public double UsagePercent
    {
        get
        {
            if (TotalMeters == 0) return 0;
            return ((TotalMeters - RemainingMeters) / TotalMeters * 100);
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