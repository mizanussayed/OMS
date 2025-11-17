using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class ClothInventoryViewModel(IDataService dataService, IAlert alertService) : ObservableObject
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
        var clothViewModels = clothsList.Select(c => new ClothInventoryItemViewModel(c, dataService, alertService, this)).ToList();
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
        
        dialog.Disappearing += async (s, e) => 
        {
            await LoadDataAsync();
        };
        
        await Shell.Current.Navigation.PushModalAsync(dialog);
    }
}

public partial class ClothInventoryItemViewModel(Cloth cloth, IDataService dataService, IAlert alertService, ClothInventoryViewModel parentViewModel) : ObservableObject
{
    public int Id => cloth.Id;
    public string UniqueCode => cloth.UniqueCode;
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
    public double ProgressWidth => UsagePercent * 2.5;

    public Color ProgressColor
    {
        get
        {
            if (IsLowStock)
                return Microsoft.Maui.Graphics.Color.FromArgb("#ea580c");
            else
                return Microsoft.Maui.Graphics.Color.FromArgb("#9333ea");
        }
    }

    [RelayCommand]
    private async Task Edit()
    {
        var dialog = new AddClothDialog
        {
            BindingContext = new AddClothViewModel(dataService, cloth)
        };
        
        //dialog.Disappearing += async (s, e) => 
        //{
        //    await parentViewModel.LoadDataAsync();
        //};
        
        await Shell.Current.Navigation.PushModalAsync(dialog);
    }

    [RelayCommand]
    private async Task Delete()
    {
        var confirmed = await alertService.DisplayConfirmAlert(
            "Delete Cloth",
            $"Are you sure you want to delete '{cloth.Name}'? This action cannot be undone.",
            "Delete",
            "Cancel");

        if (confirmed)
        {
            await dataService.DeleteClothAsync(Id);
            await alertService.DisplayAlert("Success", "Cloth deleted successfully", "OK");
            await parentViewModel.LoadDataAsync();
        }
    }

    [RelayCommand]
    private async Task ShowOptions()
    {
        var optionsSheet = new ItemOptionsSheet();
        
        optionsSheet.EditRequested += async (s, e) => await Edit();
        optionsSheet.DeleteRequested += async (s, e) => await Delete();
        
        //await Shell.Current.Navigation.PushModalAsync(optionsSheet);
    }
}