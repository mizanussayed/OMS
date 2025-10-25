using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
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
        // TODO: Open add cloth dialog/sheet
        if (Application.Current?.Windows.FirstOrDefault()?.Page != null)
        {
            await Application.Current.Windows.FirstOrDefault().Page.DisplayAlert("Add Cloth", "Add cloth feature coming soon", "OK");
        }
    }
}

// ViewModel wrapper for Cloth items with UI-specific properties
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
    public double StockProgressWidth => StockPercent * 3; // Adjust multiplier as needed
    public bool IsLowStock => RemainingMeters < TotalMeters * 0.2m;
}