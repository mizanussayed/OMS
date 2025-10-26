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
        LoadData();
    }

    [ObservableProperty]
    private ObservableCollection<ClothInventoryItemViewModel> cloths = new();

    private async void LoadData()
    {
        var clothsList = await _dataService.GetClothsAsync();
        var clothViewModels = clothsList.Select(c => new ClothInventoryItemViewModel(c)).ToList();
        Cloths = new ObservableCollection<ClothInventoryItemViewModel>(clothViewModels);
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
        LoadData();
    }
}

public class ClothInventoryItemViewModel : ObservableObject
{
    private readonly Cloth _cloth;

    public ClothInventoryItemViewModel(Cloth cloth)
    {
        _cloth = cloth;
    }

    public string Id => _cloth.Id;
    public string Name => _cloth.Name;
    public string Color => _cloth.Color;
    public decimal PricePerMeter => _cloth.PricePerMeter;
    public decimal TotalMeters => _cloth.TotalMeters;
    public decimal RemainingMeters => _cloth.RemainingMeters;
    public DateTime AddedDate => _cloth.AddedDate;

    public decimal UsedMeters => TotalMeters - RemainingMeters;
    public decimal TotalValue => RemainingMeters * PricePerMeter;
    public double StockPercent => (double)(RemainingMeters / TotalMeters * 100);
    public bool IsLowStock => RemainingMeters < TotalMeters * 0.2m;
    
    // Progress bar width (in device-independent units for the UI)
    public double ProgressWidth => StockPercent * 2.5; // Adjust multiplier based on container width
    
    // Progress bar color based on stock level
    public Microsoft.Maui.Graphics.Color ProgressColor
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