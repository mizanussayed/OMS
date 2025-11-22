using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;
using System.Security.Permissions;

namespace OMS.ViewModels;

public partial class DressOrdersViewModel(IDataService dataService, IAlert alertService) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<DressOrderItemViewModel> orders = [];
    
    private List<DressOrderItemViewModel> _allOrders = [];

    [ObservableProperty]
    private bool hasNoOrders;

    [ObservableProperty]
    private bool isRefreshing;
    
    [ObservableProperty]
    private string searchText = string.Empty;
    
    [ObservableProperty]
    private DressOrderStatus? selectedStatusFilter = null;
    
    [ObservableProperty]
    private ObservableCollection<string> availableStatuses =
    [
        "Pending", 
        "Completed", 
        "Delivered",
        "All",
    ];
    
    [ObservableProperty]
    private string selectedStatusFilterString = "Pending";
    
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }
    
    partial void OnSelectedStatusFilterStringChanged(string value)
    {
        selectedStatusFilter = value switch
        {
            "Pending" => DressOrderStatus.Pending,
            "Completed" => DressOrderStatus.Completed,
            "Delivered" => DressOrderStatus.Delivered,
            _ => null
        };
        ApplyFilters();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsRefreshing = true;
        
        try
        {
            var ordersList = await dataService.GetOrdersAsync();
            var clothsList = await dataService.GetClothsAsync();
            
            var orderViewModels = ordersList.Select(o => 
            {
                var cloth = clothsList.FirstOrDefault(c => c.Id == o.ClothId);
                return new DressOrderItemViewModel(o, cloth, dataService, alertService, this);
            }).ToList();
            
            _allOrders = orderViewModels;
            ApplyFilters();
        }
        catch
        {
        }
        finally
        {
            IsRefreshing = false;
        }
    }
    
    private void ApplyFilters()
    {
        var filtered = _allOrders.AsEnumerable();
        
        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(o => 
                o.CustomerName.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                o.MobileNumber.Contains(search) ||
                o.UniqueCode.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                o.DressType.Contains(search, StringComparison.CurrentCultureIgnoreCase));
        }
        
        // Status filter
        if (SelectedStatusFilter.HasValue)
        {
            filtered = filtered.Where(o => o.Status == SelectedStatusFilter.Value);
        }
        
        Orders = new ObservableCollection<DressOrderItemViewModel>(filtered);
        HasNoOrders = !Orders.Any();
    }
    
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedStatusFilterString = "All";
    }

    [RelayCommand]
    private async Task ShowFilters()
    {
        var filterSheet = new OrderFilterSheet(
            SearchText,
            SelectedStatusFilterString
        );
        
        filterSheet.FiltersApplied += (s, e) =>
        {
            SearchText = e.SearchText;
            SelectedStatusFilterString = e.SelectedStatus;
        };
        
        await Shell.Current.Navigation.PushModalAsync(filterSheet);
    }

    [RelayCommand]
    private async Task ShowNewOrder()
    {
        var dialog = new NewOrderDialog
        {
            BindingContext = new NewOrderViewModel(dataService, alertService)
        };
        
        dialog.Disappearing += async (s, e) => 
        {
            await LoadDataAsync();
        };
        
        await Shell.Current.Navigation.PushModalAsync(dialog);
    }

    [RelayCommand]
    private async Task Print()
    {
        try
        {

            var permissionStatus = await CheckAndRequestBluetoothPermissions();
            if (!permissionStatus)
            {
                await DisplayAlert("Permission Required",
                    "Bluetooth permissions are required to connect to the printer. Please enable them in your device settings.",
                    "OK");
                return;
            }

            var devices = await _printerService.GetPairedDevicesAsync();

            if (devices.Count == 0)
            {
                await DisplayAlert("Error", "No paired Bluetooth devices found. Please pair your printer first.", "OK");
                return;
            }

            var selectedDevice = await DisplayActionSheet("Select Printer", "Cancel", null, devices.ToArray());

            if (selectedDevice == "Cancel" || string.IsNullOrEmpty(selectedDevice))
                return;

            loadingLabel.Text = $"Connecting to {selectedDevice}...";
            loadingOverlay.IsVisible = true;
            await Task.Delay(50);

            var connected = await _printerService.ConnectAsync(selectedDevice);

            if (!connected)
            {
                loadingOverlay.IsVisible = false;
                await DisplayAlert("Error", "Failed to connect to printer.", "OK");
                return;
            }

            loadingLabel.Text = "Printing invoice...";
            await Task.Delay(50);

            // Fast text printing
            var printed = await PrintInvoiceTextFast();

            loadingOverlay.IsVisible = false;

            if (printed)
            {
                await DisplayAlert("Success", "Invoice printed successfully!", "OK");
                await Navigation.PopModalAsync();
                await Shell.Current.GoToAsync(nameof(NewOrderListPage));
            }
            else
            {
                await DisplayAlert("Error", "Failed to print. Please check the printer and try again.", "OK");
            }

            await _printerService.DisconnectAsync();
        }
        catch (Exception ex)
        {
            loadingOverlay.IsVisible = false;
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
        finally
        {
            loadingOverlay.IsVisible = false;
            btnPrint.IsEnabled = true;
        }
    }

    private async Task<bool> PrintInvoiceTextFast()
    {
        try
        {
            // Header lines with larger font
            var headerLines = new List<string>
            {
                "YOUSUF TAILOR",
            };

            var headerPrinted = await _printerService.PrintFormattedTextAsync(headerLines, fontSize: 18, centerAlign: true, false);
            if (!headerPrinted)
                return false;

            var bodyLines = new List<string>
            {
                "Phone: 01730298184",
                "Brahmanbaria Hawkers Market",
                "",
                "--------------------------------",
                "          Advance Receipt       ",
                "--------------------------------",
                $"Customer: {_order.CustomerName}",
                $"Mobile    : {_order.MobileNumber}",
                $"Order Type     : {_order.OrderFor}",
                "",
                $"Total Amount   : {_order.TotalAmount}/-",
                $"Paid Amount    : {_order.PaidAmount}/-",
                $"Due Amount     : {_order.DueAmount}/-",
                $"Delivery Date  : {_order.DeliveryDate:dd MMM yyyy}",
                "",
                "--------------------------------",
                "",
                "Thank you! Come again.",
                "",
                "",
            };

            return await _printerService.PrintFormattedTextAsync(bodyLines, fontSize: 12, centerAlign: true);
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> CheckAndRequestBluetoothPermissions()
    {
#if ANDROID
        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
        {
            var connectStatus = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
            if (connectStatus != PermissionStatus.Granted)
            {
                connectStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();
                if (connectStatus != PermissionStatus.Granted)
                    return false;
            }
        }
        else
        {
            var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (locationStatus != PermissionStatus.Granted)
                    return false;
            }
        }
#endif
        return true;
    }
}

public partial class DressOrderItemViewModel(DressOrder order, 
    Cloth? cloth,
    IDataService dataService,
    IAlert alertService, 
    DressOrdersViewModel parentViewModel)
    : ObservableObject
{
    private DressOrderStatus _status = order.Status;

    public int Id => order.Id;
    public string UniqueCode => order.UniqueCode;
    public string CustomerName => order.CustomerName + " (" + order.UniqueCode + ")";
    public string MobileNumber => order.MobileNumber;
    public string DressType => order.DressType;
    public int ClothId => order.ClothId;
    public double MetersUsed => order.MetersUsed;
    public DressOrderStatus Status => _status;
    public int? AssignedTo => order.AssignedTo;
    public DateTime OrderDate => order.OrderDate;

    // Cloth properties
    public string AssignedToName => "Manager";
    public string ClothName => cloth!.Name + " (" + cloth!.UniqueCode + ")";
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
    
    // Can only edit or delete pending orders
    public bool CanEditOrDelete => IsPending;

    public bool HasAssignedTo => AssignedTo > 0;

    [RelayCommand]
    private async Task Complete()
    {
        if (!IsPending)
        {
            await alertService.DisplayAlert("Cannot Complete", "Only pending orders can be marked as completed.", "OK");
            return;
        }
        
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
        OnPropertyChanged(nameof(CanEditOrDelete));
    }

    [RelayCommand]
    private async Task Deliver()
    {
        if (!IsCompleted)
        {
            await alertService.DisplayAlert("Cannot Deliver", "Only completed orders can be marked as delivered.", "OK");
            return;
        }
        
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
        OnPropertyChanged(nameof(CanEditOrDelete));
    }

    [RelayCommand]
    private async Task Edit()
    {
        // Only allow editing pending orders
        if (!IsPending)
        {
            await alertService.DisplayAlert(
                "Cannot Edit", 
                "Only pending orders can be edited. This order has already been processed.", 
                "OK");
            return;
        }
        
        var dialog = new NewOrderDialog
        {
            BindingContext = new NewOrderViewModel(dataService, alertService, order)
        };
        
        dialog.Disappearing += async (s, e) => 
        {
            await Task.Delay(100);
            await parentViewModel.LoadDataAsync();
        };
        
        await Shell.Current.Navigation.PushModalAsync(dialog);
    }

    [RelayCommand]
    private async Task Delete()
    {
        // Only allow deleting pending orders
        if (!IsPending)
        {
            await alertService.DisplayAlert(
                "Cannot Delete", 
                "Only pending orders can be deleted. This order has already been processed.", 
                "OK");
            return;
        }
        
        var confirmed = await alertService.DisplayConfirmAlert(
            "Delete Order",
            $"Are you sure you want to delete order for '{order.CustomerName}'? This action cannot be undone.",
            "Delete",
            "Cancel");

        if (confirmed)
        {
            // Return the meters to cloth stock when deleting order
            if (cloth != null)
            {
                await dataService.UpdateClothRemainingMetersAsync(ClothId, -MetersUsed);
            }
            
            await dataService.DeleteOrderAsync(Id);
            await alertService.DisplayAlert("Success", "Order deleted successfully", "OK");
            await parentViewModel.LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task ShowOptions()
    {
        // Only show edit/delete options for pending orders
        if (!IsPending)
        {
            await alertService.DisplayAlert(
                "Order Processed", 
                "This order has already been processed and cannot be edited or deleted.\n\n" +
                $"Status: {StatusText}\n" +
                $"Customer: {order.CustomerName}\n" +
                $"Order Date: {OrderDate:MMM dd, yyyy}", 
                "OK");
            return;
        }
        
        var optionsSheet = new ItemOptionsSheet();
        
        optionsSheet.EditRequested += async (s, e) => await Edit();
        optionsSheet.DeleteRequested += async (s, e) => await Delete();
        
        await Shell.Current.Navigation.PushModalAsync(optionsSheet);
    }
}
