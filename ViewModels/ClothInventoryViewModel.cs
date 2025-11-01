using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class ClothInventoryViewModel(IDataService dataService) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ClothInventoryItemViewModel> cloths = [];


    [ObservableProperty]
    private bool hasNoCloth;

    [ObservableProperty]
    private bool isRefreshing;

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsRefreshing = true;
        var clothsList = await dataService.GetClothsAsync();
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
            BindingContext = new AddClothViewModel(dataService)
        };
        await Shell.Current.Navigation.PushModalAsync(dialog);

        await LoadDataAsync();
    }
}

public class ClothInventoryItemViewModel(Cloth cloth) : ObservableObject
{
    public int Id => cloth.Id;
    public string Name => cloth.Name + " (" + cloth.UniqueCode + ")";
    public string Color => cloth.Color;
    public double PricePerMeter => cloth.PricePerMeter;
    public double TotalMeters => cloth.TotalMeters;
    public double RemainingMeters => cloth.RemainingMeters;
    public DateTime AddedDate => cloth.AddedDate;

    public double UsedMeters => TotalMeters - RemainingMeters;
    public double TotalValue => RemainingMeters * PricePerMeter;
    public double UsagePercent => ((TotalMeters - RemainingMeters) / TotalMeters * 100);
    public bool IsLowStock => RemainingMeters < TotalMeters * 0.2;
    public double ProgressWidth => UsagePercent * 2.5; // Changed from StockPercent to UsagePercent

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