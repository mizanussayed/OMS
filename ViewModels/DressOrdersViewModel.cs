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
    private ObservableCollection<DressOrderItemViewModel> orders = new();

    [ObservableProperty]
    private bool hasNoOrders;

    [ObservableProperty]
    private bool isRefreshing;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsRefreshing = true;
        var ordersList = await dataService.GetOrdersAsync();
        var clothsList = await dataService.GetClothsAsync();
        
        var orderViewModels = ordersList.Select(o => 
        {
            var cloth = clothsList.FirstOrDefault(c => c.Id == o.ClothId);
            return new DressOrderItemViewModel(o, cloth, dataService);
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

public partial class DressOrderItemViewModel(DressOrder order, Cloth? cloth, IDataService dataService) : ObservableObject
{
    private DressOrderStatus _status = order.Status;

    public int Id => order.Id;
    public string CustomerName => order.CustomerName;
    public string DressType => order.DressType;
    public int ClothId => order.ClothId;
    public double MetersUsed => order.MetersUsed;
    public DressOrderStatus Status => _status;
    public int? AssignedTo => order.AssignedTo;
    public DateTime OrderDate => order.OrderDate;

    // Cloth properties
    public string ClothName => cloth?.Name ?? "Unknown";
    public string ClothColor => cloth?.Color ?? "Unknown";
    public double TotalCost => cloth != null ? MetersUsed * cloth.PricePerMeter : 0;

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

    public bool HasAssignedTo => AssignedTo > 0;

    [RelayCommand]
    private async Task Complete()
    {
        await dataService.UpdateOrderStatusAsync(Id, DressOrderStatus.Completed);
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
        await dataService.UpdateOrderStatusAsync(Id, DressOrderStatus.Delivered);
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
}
