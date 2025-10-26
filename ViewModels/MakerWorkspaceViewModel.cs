using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class MakerWorkspaceViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly string _makerId;

    [ObservableProperty]
    private string makerName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DressOrderItemViewModel> orders = new();

    [ObservableProperty]
    private int totalOrders;

    [ObservableProperty]
    private int pendingOrders;

    [ObservableProperty]
    private bool hasNoOrders;

    public MakerWorkspaceViewModel(IDataService dataService, string makerId, string makerName)
    {
        _dataService = dataService;
        _makerId = makerId;
        MakerName = makerName;
        LoadData();
    }

    private async void LoadData()
    {
        var allOrders = await _dataService.GetOrdersAsync();
        var clothsList = await _dataService.GetClothsAsync();
        
        // Filter orders assigned to this maker
        var myOrders = allOrders.Where(o => o.AssignedTo == _makerId).ToList();
        
        var orderViewModels = myOrders.Select(o => 
        {
            var cloth = clothsList.FirstOrDefault(c => c.Id == o.ClothId);
            return new DressOrderItemViewModel(o, cloth, _dataService);
        }).ToList();
        
        Orders = new ObservableCollection<DressOrderItemViewModel>(orderViewModels);
        HasNoOrders = !Orders.Any();
        TotalOrders = Orders.Count;
        PendingOrders = Orders.Count(o => o.IsPending);
    }

    [RelayCommand]
    private async Task Logout()
    {
        // Clear current user
        App.CurrentUser = null;

        // Navigate to login
        await Shell.Current.GoToAsync("//login");
    }
}
