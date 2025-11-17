using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class MakerWorkspaceViewModel : ObservableObject
{
    private readonly IDataService dataService;
    private readonly IAlert alertService;
    private readonly int _makerId;

    [ObservableProperty]
    private string makerName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<DressOrderItemViewModel> orders = [];

    [ObservableProperty]
    private int totalOrders;

    [ObservableProperty]
    private int pendingOrders;

    [ObservableProperty]
    private bool hasNoOrders;

    public MakerWorkspaceViewModel(IDataService dataService, IAlert alertService, int makerId, string makerName)
    {
        this.dataService = dataService;
        this.alertService = alertService;
        _makerId = makerId;
        MakerName = makerName;
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        var allOrders = await dataService.GetOrdersAsync();
        var clothsList = await dataService.GetClothsAsync();

        var myOrders = allOrders.Where(o => o.AssignedTo == _makerId).ToList();

        var orderViewModels = myOrders.Select(o =>
        {
            var cloth = clothsList.FirstOrDefault(c => c.Id == o.ClothId);
            return new DressOrderItemViewModel(o, cloth, dataService, alertService, null!);
        }).ToList();

        Orders = new ObservableCollection<DressOrderItemViewModel>(orderViewModels);
        HasNoOrders = !Orders.Any();
        TotalOrders = Orders.Count;
        PendingOrders = Orders.Count(o => o.IsPending);
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
