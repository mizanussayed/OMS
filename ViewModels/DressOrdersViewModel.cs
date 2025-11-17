using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class DressOrdersViewModel(IDataService dataService, IAlert alertService) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<DressOrderItemViewModel> orders = [];

    [ObservableProperty]
    private bool hasNoOrders;

    [ObservableProperty]
    private bool isRefreshing;

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsRefreshing = true;
        var ordersList = await dataService.GetOrdersAsync();
        var clothsList = await dataService.GetClothsAsync();
        
        var orderViewModels = ordersList.Select(o => 
        {
            var cloth = clothsList.FirstOrDefault(c => c.Id == o.ClothId);
            return new DressOrderItemViewModel(o, cloth, dataService, alertService, this);
        }).ToList();
        
        Orders = new ObservableCollection<DressOrderItemViewModel>(orderViewModels);
        HasNoOrders = !Orders.Any();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task ShowNewOrder()
    {
        var dialog = new NewOrderDialog
        {
            BindingContext = new NewOrderViewModel(dataService, alertService)
        };
        await Shell.Current.Navigation.PushModalAsync(dialog);
  
        await LoadDataAsync();
    }
}

public partial class DressOrderItemViewModel : ObservableObject
{
    private readonly DressOrder _order;
    private readonly Cloth? _cloth;
    private readonly IDataService _dataService;
    private readonly IAlert _alertService;
    private readonly DressOrdersViewModel _parentViewModel;
    private DressOrderStatus _status;

    public DressOrderItemViewModel(DressOrder order, Cloth? cloth, IDataService dataService, IAlert alertService, DressOrdersViewModel parentViewModel)
    {
        _order = order;
        _cloth = cloth;
        _dataService = dataService;
        _alertService = alertService;
        _parentViewModel = parentViewModel;
        _status = order.Status;
    }

    public int Id => _order.Id;
    public string UniqueCode => _order.UniqueCode;
    public string CustomerName => _order.CustomerName + " (" + _order.UniqueCode + ")";
    public string MobileNumber => _order.MobileNumber;
    public string DressType => _order.DressType;
    public int ClothId => _order.ClothId;
    public double MetersUsed => _order.MetersUsed;
    public DressOrderStatus Status => _status;
    public int? AssignedTo => _order.AssignedTo;
    public DateTime OrderDate => _order.OrderDate;

    // Cloth properties
    public string AssignedToName => "Manager";
    public string ClothName => _cloth!.Name + " (" + _cloth!.UniqueCode + ")";
    public string ClothColor => _cloth?.Color ?? "Unknown";
    public double TotalCost => _cloth != null ? MetersUsed * _cloth.PricePerMeter : 0;

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
        DressOrderStatus.Pending => Color.FromArgb("#fef3c7"),
        DressOrderStatus.Completed => Color.FromArgb("#dbeafe"),
        DressOrderStatus.Delivered => Color.FromArgb("#dcfce7"),
        _ => Colors.Gray
    };

    public Color StatusBorderColor => Status switch
    {
        DressOrderStatus.Pending => Color.FromArgb("#fde68a"),
        DressOrderStatus.Completed => Color.FromArgb("#bfdbfe"),
        DressOrderStatus.Delivered => Color.FromArgb("#bbf7d0"),
        _ => Colors.Gray
    };

    public Color StatusTextColor => Status switch
    {
        DressOrderStatus.Pending => Color.FromArgb("#854d0e"),
        DressOrderStatus.Completed => Color.FromArgb("#1e40af"),
        DressOrderStatus.Delivered => Color.FromArgb("#166534"),
        _ => Colors.Black
    };

    // Visibility properties
    public bool IsPending => Status == DressOrderStatus.Pending;
    public bool IsCompleted => Status == DressOrderStatus.Completed;
    public bool IsDelivered => Status == DressOrderStatus.Delivered;

    public bool HasAssignedTo => AssignedTo > 0;

    [RelayCommand]
    private async Task Complete()
    {
        await _dataService.UpdateOrderStatusAsync(Id, DressOrderStatus.Completed);
        _status = DressOrderStatus.Completed;
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusBackgroundColor));
        OnPropertyChanged(nameof(StatusBorderColor));
        OnPropertyChanged(nameof(StatusTextColor));
        OnPropertyChanged(nameof(IsPending));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(IsDelivered));
    }

    [RelayCommand]
    private async Task Deliver()
    {
        await _dataService.UpdateOrderStatusAsync(Id, DressOrderStatus.Delivered);
        _status = DressOrderStatus.Delivered;
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusBackgroundColor));
        OnPropertyChanged(nameof(StatusBorderColor));
        OnPropertyChanged(nameof(StatusTextColor));
        OnPropertyChanged(nameof(IsPending));
        OnPropertyChanged(nameof(IsCompleted));
        OnPropertyChanged(nameof(IsDelivered));
    }

    [RelayCommand]
    private async Task Edit()
    {
        var dialog = new NewOrderDialog
        {
            BindingContext = new NewOrderViewModel(_dataService, _alertService, _order)
        };
        await Shell.Current.Navigation.PushModalAsync(dialog);
        await _parentViewModel.LoadDataAsync();
    }

    [RelayCommand]
    private async Task Delete()
    {
        var confirmed = await _alertService.DisplayConfirmAlert(
            "Delete Order",
            $"Are you sure you want to delete order for '{_order.CustomerName}'? This action cannot be undone.",
            "Delete",
            "Cancel");

        if (confirmed)
        {
            // Return the meters to cloth stock when deleting order
            if (_cloth != null)
            {
                await _dataService.UpdateClothRemainingMetersAsync(ClothId, -MetersUsed);
            }
            
            await _dataService.DeleteOrderAsync(Id);
            await _alertService.DisplayAlert("Success", "Order deleted successfully", "OK");
            await _parentViewModel.LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task ShowOptions()
    {
        var optionsSheet = new ItemOptionsSheet();
        
        optionsSheet.EditRequested += async (s, e) => await Edit();
        optionsSheet.DeleteRequested += async (s, e) => await Delete();
        
        await Shell.Current.Navigation.PushModalAsync(optionsSheet);
    }
}
