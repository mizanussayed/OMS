using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class DressOrdersViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    public DressOrdersViewModel(IDataService dataService)
    {
        _dataService = dataService;
        LoadData();
    }

    [ObservableProperty]
    private ObservableCollection<DressOrderItemViewModel> orders = new();

    [ObservableProperty]
    private bool hasNoOrders;

    private async void LoadData()
    {
        var ordersList = await _dataService.GetOrdersAsync();
        var clothsList = await _dataService.GetClothsAsync();
        
        var orderViewModels = ordersList.Select(o => 
        {
            var cloth = clothsList.FirstOrDefault(c => c.Id == o.ClothId);
            return new DressOrderItemViewModel(o, cloth, _dataService);
        }).ToList();
        
        Orders = new ObservableCollection<DressOrderItemViewModel>(orderViewModels);
        HasNoOrders = !Orders.Any();
    }

    [RelayCommand]
    private async Task ShowNewOrder()
    {
        var dialog = new NewOrderDialog
        {
            BindingContext = new NewOrderViewModel(_dataService)
        };
        await Shell.Current.Navigation.PushModalAsync(dialog);
        
        // Reload data when dialog closes
        await Task.Delay(500);
        LoadData();
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
}

// ViewModel wrapper for DressOrder items with UI-specific properties
public partial class DressOrderItemViewModel : ObservableObject
{
    private readonly DressOrder _order;
    private readonly Cloth? _cloth;
    private readonly IDataService _dataService;

    public DressOrderItemViewModel(DressOrder order, Cloth? cloth, IDataService dataService)
    {
        _order = order;
        _cloth = cloth;
        _dataService = dataService;
    }

    public string Id => _order.Id;
    public string CustomerName => _order.CustomerName;
    public string DressType => _order.DressType;
    public string ClothId => _order.ClothId;
    public decimal MetersUsed => _order.MetersUsed;
    public DressOrderStatus Status => _order.Status;
    public string? AssignedTo => _order.AssignedTo;
    public DateTime OrderDate => _order.OrderDate;

    // Cloth properties
    public string ClothName => _cloth?.Name ?? "Unknown";
    public string ClothColor => _cloth?.Color ?? "Unknown";
    public decimal TotalCost => _cloth != null ? MetersUsed * _cloth.PricePerMeter : 0;

    // Status properties
    public string StatusText => Status switch
    {
        DressOrderStatus.Pending => "Pending",
        DressOrderStatus.Completed => "Completed",
        DressOrderStatus.Delivered => "Delivered",
        _ => "Unknown"
    };

    public Color StatusBackgroundColor => Status switch
    {
        DressOrderStatus.Pending => Color.FromArgb("#fef3c7"), // Yellow100
        DressOrderStatus.Completed => Color.FromArgb("#dbeafe"), // Blue100
        DressOrderStatus.Delivered => Color.FromArgb("#dcfce7"), // Green100
        _ => Colors.Gray
    };

    public Color StatusBorderColor => Status switch
    {
        DressOrderStatus.Pending => Color.FromArgb("#fde68a"), // Yellow200
        DressOrderStatus.Completed => Color.FromArgb("#bfdbfe"), // Blue200
        DressOrderStatus.Delivered => Color.FromArgb("#bbf7d0"), // Green200
        _ => Colors.Gray
    };

    public Color StatusTextColor => Status switch
    {
        DressOrderStatus.Pending => Color.FromArgb("#854d0e"), // Yellow800
        DressOrderStatus.Completed => Color.FromArgb("#1e40af"), // Blue800
        DressOrderStatus.Delivered => Color.FromArgb("#166534"), // Green800
        _ => Colors.Black
    };

    // Visibility properties
    public bool IsPending => Status == DressOrderStatus.Pending;
    public bool IsCompleted => Status == DressOrderStatus.Completed;
    public bool IsDelivered => Status == DressOrderStatus.Delivered;

    // Assigned To
    public bool HasAssignedTo => !string.IsNullOrEmpty(AssignedTo);
    public string AssignedToName => AssignedTo switch
    {
        "maker-1" => "Rajesh Kumar",
        "maker-2" => "Amit Patel",
        "maker-3" => "Suresh Singh",
        _ => "Unassigned"
    };

    // Commands
    [RelayCommand]
    private async Task Complete()
    {
        await _dataService.UpdateOrderStatusAsync(Id, DressOrderStatus.Completed);
        // Reload or update UI
    }

    [RelayCommand]
    private async Task Deliver()
    {
        await _dataService.UpdateOrderStatusAsync(Id, DressOrderStatus.Delivered);
        // Reload or update UI
    }
}
