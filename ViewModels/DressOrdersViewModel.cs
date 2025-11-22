using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class DressOrdersViewModel(IDataService dataService, IAlert alertService, IBluetoothPrinterService printerService) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<DressOrderItemViewModel> orders = [];
    
    private List<DressOrderItemViewModel> _allOrders = [];

    [ObservableProperty]
    private bool hasNoOrders;

    [ObservableProperty]
    private bool isRefreshing;
    
    [ObservableProperty]
    private bool isPrinting;
    
    [ObservableProperty]
    private string printingStatus = string.Empty;
    
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
            IsPrinting = true;
            PrintingStatus = "Checking permissions...";
            
            var permissionStatus = await CheckAndRequestBluetoothPermissions();
            if (!permissionStatus)
            {
                IsPrinting = false;
                await alertService.DisplayAlert("Permission Required",
                    "Enable Bluetooth in your device settings.",
                    "OK");
                return;
            }

            PrintingStatus = "Searching for devices...";
            var devices = await printerService.GetPairedDevicesAsync();

            if (devices.Count == 0)
            {
                IsPrinting = false;
                await alertService.DisplayAlert("Error", "No paired Bluetooth devices found.", "OK");
                return;
            }

            // Show device selection sheet
            IsPrinting = false;
            var deviceArray = devices.ToArray();
            var selectedDeviceName = await ShowDeviceSelectionDialog(deviceArray);

            if (string.IsNullOrEmpty(selectedDeviceName))
                return;

            // Start printing process
            IsPrinting = true;
            PrintingStatus = $"Connecting to {selectedDeviceName}...";
            
            var connected = await printerService.ConnectAsync(selectedDeviceName);

            if (!connected)
            {
                IsPrinting = false;
                await alertService.DisplayAlert("Error", "Failed to connect to printer.", "OK");
                return;
            }

            PrintingStatus = "Printing orders...";
            
            // Print all orders report
            var printed = await PrintOrdersReport();

            IsPrinting = false;
            
            if (printed)
            {
                await alertService.DisplayAlert("Success", "Orders printed successfully!", "OK");
            }
            else
            {
                await alertService.DisplayAlert("Error", "Failed to print.", "OK");
            }

            await printerService.DisconnectAsync();
        }
        catch (Exception ex)
        {
            IsPrinting = false;
            await alertService.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private static async Task<string?> ShowDeviceSelectionDialog(string[] devices)
    {
        if (devices.Length == 0)
            return null;

        if (devices.Length == 1)
            return devices[0];

        // Create and show device selection sheet
        var tcs = new TaskCompletionSource<string?>();

        var deviceSheet = new DeviceSelectionSheet(devices);
        deviceSheet.DeviceSelected += (s, deviceName) =>
        {
            tcs.TrySetResult(deviceName);
        };

        deviceSheet.Disappearing += (s, e) =>
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.TrySetResult(null);
            }
        };

        await Shell.Current.Navigation.PushModalAsync(deviceSheet);

        return await tcs.Task;
    }

    private async Task<bool> PrintOrdersReport()
    {
        try
        {
            var filteredOrders = Orders.ToList();
            
            if (filteredOrders.Count == 0)
            {
                await alertService.DisplayAlert("No Orders", "No orders to print.", "OK");
                return false;
            }

            // Print each order
            foreach (var order in filteredOrders)
            {
                // Header
                var headerLines = new List<string>
                {
                    $"{order.UniqueCode.ToUpper()}",
                };

                var headerPrinted = await printerService.PrintFormattedTextAsync(headerLines, fontSize: 18, centerAlign: true, false);
                if (!headerPrinted)
                    return false;

                var orderLines = new List<string>
                {
                    $"Customer: {order.CustomerName.Replace($" ({order.UniqueCode})", "")}",
                    $"Mobile: {order.MobileNumber}",
                    $"Dress Type: {order.DressType}",
                    $"Cloth: {order.ClothName}",
                    $"Color: {order.ClothColor}",
                    $"Meters Used: {order.MetersUsed}m",
                    $"Cost: BDT {order.TotalCost:N2}",
                    $"Order Date: {order.OrderDate:dd MMM yyyy}",
                    "--------------------------------",
                    ""
                };
                var orderPrinted = await printerService.PrintFormattedTextAsync(orderLines, fontSize: 10, centerAlign: false, true);
                if (!orderPrinted)
                    return false;
            }
            return true;
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
            await alertService.DisplayAlert("Cannot Complete", "Only pending orders can be completed.", "OK");
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
            await alertService.DisplayAlert("Cannot Deliver", "Only completed orders can be delivered.", "OK");
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
                "Only pending orders can be edited.", 
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
                "Only pending orders can be deleted", 
                "OK");
            return;
        }
        
        var confirmed = await alertService.DisplayConfirmAlert(
            "Delete Order",
            $"Are you sure you want to delete order for '{order.CustomerName}'?",
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
        if (!IsPending)
        {
            await alertService.DisplayAlert(
                "Order Processed", 
                "This order has been processed, cannot be edit/delete",
                "OK");
            return;
        }
        
        var optionsSheet = new ItemOptionsSheet();
        
        optionsSheet.EditRequested += async (s, e) => await Edit();
        optionsSheet.DeleteRequested += async (s, e) => await Delete();
        
        await Shell.Current.Navigation.PushModalAsync(optionsSheet);
    }
}
