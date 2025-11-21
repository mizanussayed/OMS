using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class MakerWorkspaceViewModel : ObservableObject
{
    private readonly IDataService dataService;
    private readonly IAlert alertService;
    private readonly int _makerId;
    private List<DressOrderItemViewModel> _allOrders = [];

    [ObservableProperty]
    private string makerName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DressOrderItemViewModel> orders = [];

    [ObservableProperty]
    private DressOrderStatus? selectedStatusFilter = null;

    [ObservableProperty]
    private ObservableCollection<string> availableStatuses = new()
    {
        "All",
        "Pending",
        "Completed",
        "Delivered"
    };

    [ObservableProperty]
    private string selectedStatusFilterString = "All";

    [ObservableProperty]
    private int totalOrders;

    [ObservableProperty]
    private int pendingOrders;

    [ObservableProperty]
    private int completedOrders;

    [ObservableProperty]
    private int deliveredOrders;

    [ObservableProperty]
    private bool hasNoOrders;

    public MakerWorkspaceViewModel(IDataService dataService, IAlert alertService, int makerId, string makerName)
    {
        this.dataService = dataService;
        this.alertService = alertService;
        _makerId = makerId;
        MakerName = makerName;
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
        var allOrders = await dataService.GetOrdersByEmployeeAsync(_makerId);
        var clothsList = await dataService.GetClothsAsync();

        var orderViewModels = allOrders.Select(o =>
        {
            var cloth = clothsList.FirstOrDefault(c => c.Id == o.ClothId);
            return new DressOrderItemViewModel(o, cloth, dataService, alertService, null!);
        }).ToList();

        _allOrders = orderViewModels;

        // Update stats
        TotalOrders = _allOrders.Count;
        PendingOrders = _allOrders.Count(o => o.IsPending);
        CompletedOrders = _allOrders.Count(o => o.IsCompleted);
        DeliveredOrders = _allOrders.Count(o => o.IsDelivered);

        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allOrders.AsEnumerable();

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
        SelectedStatusFilterString = "All";
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
}
