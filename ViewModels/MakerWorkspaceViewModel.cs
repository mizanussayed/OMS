using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class MakerWorkspaceViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly int _makerId;

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

    public MakerWorkspaceViewModel(IDataService dataService, int makerId, string makerName)
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
    private static async Task Logout()
    {
        App.CurrentUser = null;

        await Shell.Current.GoToAsync("//login");
    }
}
