using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class ClothInventoryViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    public ClothInventoryViewModel(IDataService dataService)
    {
        _dataService = dataService;
        LoadDataAsync();
    }

    [ObservableProperty]
    private ObservableCollection<ClothInventoryItemViewModel> cloths = new();


    [ObservableProperty]
    private bool hasNoCloth;

    [ObservableProperty]
    private bool isRefreshing;

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsRefreshing = true;
        var clothsList = await _dataService.GetClothsAsync();
        var clothViewModels = clothsList.Select(c => new ClothInventoryItemViewModel(c)).ToList();
        Cloths = new ObservableCollection<ClothInventoryItemViewModel>(clothViewModels);
        HasNoCloth = !Cloths.Any();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task ShowAddCloth()
    {
        var dialog = new AddClothDialog
        {
            BindingContext = new AddClothViewModel(_dataService)
        };
        await Shell.Current.Navigation.PushModalAsync(dialog);
        
        await Task.Delay(500);
        await LoadDataAsync();
    }
}

public class ClothInventoryItemViewModel : ObservableObject
{
    private readonly Cloth _cloth;

    public ClothInventoryItemViewModel(Cloth cloth)
    {
        _cloth = cloth;
    }

    public int Id => _cloth.Id;
    public string Name => _cloth.Name;
    public string Color => _cloth.Color;
    public double PricePerMeter => _cloth.PricePerMeter;
    public double TotalMeters => _cloth.TotalMeters;
    public double RemainingMeters => _cloth.RemainingMeters;
    public DateTime AddedDate => _cloth.AddedDate;

    public double UsedMeters => TotalMeters - RemainingMeters;
    public double TotalValue => RemainingMeters * PricePerMeter;
    public double StockPercent => (RemainingMeters / TotalMeters * 100);
    public bool IsLowStock => RemainingMeters < TotalMeters * 0.2;
    public double ProgressWidth => StockPercent * 2.5; // Adjust multiplier based on container width
    
    public Color ProgressColor
    {
        get
        {
            if (IsLowStock)
                return Microsoft.Maui.Graphics.Color.FromArgb("#ea580c"); // Orange for low stock
            else
                return Microsoft.Maui.Graphics.Color.FromArgb("#9333ea"); // Purple for normal stock
        }
    }
}