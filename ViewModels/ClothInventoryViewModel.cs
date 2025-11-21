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
    
    private List<ClothInventoryItemViewModel> _allCloths = [];

    [ObservableProperty]
    private bool hasNoCloth;

    [ObservableProperty]
    private bool isRefreshing;
    
    [ObservableProperty]
    private string searchText = string.Empty;
    
    [ObservableProperty]
    private string selectedColorFilter = "All";
    
    [ObservableProperty]
    private ObservableCollection<string> availableColors = new() { "All" };
    
    [ObservableProperty]
    private bool isLowStockFilterActive;

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }
    
    partial void OnSelectedColorFilterChanged(string value)
    {
        ApplyFilters();
    }
    
    partial void OnIsLowStockFilterActiveChanged(bool value)
    {
        ApplyFilters();
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsRefreshing = true;
        
        try
        {
            var clothsList = await dataService.GetClothsAsync();
            var clothViewModels = clothsList.Select(c => new ClothInventoryItemViewModel(c, dataService, alertService, this)).ToList();
            _allCloths = clothViewModels;
            var colors = new List<string> { "All" };
            colors.AddRange(clothsList.Select(c => c.Color).Distinct().OrderBy(c => c));
            AvailableColors = new ObservableCollection<string>(colors);
            
            ApplyFilters();
        }
        catch (Exception ex)
        {
            // Log error but don't crash
            System.Diagnostics.Debug.WriteLine($"Error loading cloths: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }
    

    public async Task RefreshSingleClothAsync(int clothId)
    {
        try
        {
            var updatedCloth = await dataService.GetClothByIdAsync(clothId);
            if (updatedCloth != null)
            {
                var existingItemInAll = _allCloths.FirstOrDefault(c => c.Id == clothId);
                if (existingItemInAll != null)
                {
                    var index = _allCloths.IndexOf(existingItemInAll);
                    var newItem = new ClothInventoryItemViewModel(updatedCloth, dataService, alertService, this);
                    _allCloths[index] = newItem;
                }
                
                // Reapply filters
                ApplyFilters();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing cloth {clothId}: {ex.Message}");
        }
    }
    
    public void RemoveCloth(int clothId)
    {
        var itemInAll = _allCloths.FirstOrDefault(c => c.Id == clothId);
        if (itemInAll != null)
        {
            _allCloths.Remove(itemInAll);
        }
        
        // Reapply filters
        ApplyFilters();
    }
    
    private void ApplyFilters()
    {
        var filtered = _allCloths.AsEnumerable();
        
        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(c => 
                c.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                c.Color.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                c.UniqueCode.Contains(search, StringComparison.CurrentCultureIgnoreCase));
        }
        
        // Color filter
        if (SelectedColorFilter != "All")
        {
            filtered = filtered.Where(c => c.Color == SelectedColorFilter);
        }
        
        // Low stock filter
        if (IsLowStockFilterActive)
        {
            filtered = filtered.Where(c => c.IsLowStock);
        }
        
        Cloths = new ObservableCollection<ClothInventoryItemViewModel>(filtered);
        HasNoCloth = !Cloths.Any();
    }
    
    [RelayCommand]
    private void ClearFilters()
    {
        SearchText = string.Empty;
        SelectedColorFilter = "All";
        IsLowStockFilterActive = false;
    }
    
    [RelayCommand]
    private void ToggleLowStockFilter()
    {
        IsLowStockFilterActive = !IsLowStockFilterActive;
    }

    [RelayCommand]
    private async Task ShowFilters()
    {
        var filterSheet = new ClothFilterSheet(
            SearchText,
            SelectedColorFilter,
            IsLowStockFilterActive,
            [.. AvailableColors]
        );
        
        filterSheet.FiltersApplied += (s, e) =>
        {
            SearchText = e.SearchText;
            SelectedColorFilter = e.SelectedColor;
            IsLowStockFilterActive = e.IsLowStockOnly;
        };
        
        await Shell.Current.Navigation.PushModalAsync(filterSheet);
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

    [RelayCommand]
    private async Task Print()
    {
        // Implementation for printing cloth inventory report
        await alertService.DisplayAlert("Print", "Print functionality is not implemented yet.", "OK");
    }
}

public partial class ClothInventoryItemViewModel(Cloth cloth, IDataService dataService, IAlert alertService, ClothInventoryViewModel parentViewModel) : ObservableObject
{
    private Cloth _cloth = cloth;
    
    public int Id => _cloth.Id;
    public string UniqueCode => _cloth.UniqueCode;
    public string Name => _cloth.Name + " (" + _cloth.UniqueCode + ")";
    public string Color => _cloth.Color;
    public double PricePerMeter => _cloth.PricePerMeter;
    public double TotalMeters => _cloth.TotalMeters;
    public double RemainingMeters => _cloth.RemainingMeters;
    public DateTime AddedDate => _cloth.AddedDate;

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
        try
        {
            var viewModel = new AddClothViewModel(dataService, _cloth);
            var dialog = new AddClothDialog
            {
                BindingContext = viewModel
            };
            
            dialog.Disappearing += async (s, e) => 
            {
                await Task.Delay(100); // Ensure dialog is fully closed
                await parentViewModel.RefreshSingleClothAsync(Id);
            };
            
            await Shell.Current.Navigation.PushModalAsync(dialog);
        }
        catch
        {
            await alertService.DisplayAlert("Error", "An error occurred while trying to edit the cloth", "OK");
        }
    }

    [RelayCommand]
    private async Task Delete()
    {
        var confirmed = await alertService.DisplayConfirmAlert(
            "Delete Cloth",
            $"Are you sure you want to delete '{_cloth.Name}'? This action cannot be undone.",
            "Delete",
            "Cancel");

        if (confirmed)
        {
            try
            {
                await dataService.DeleteClothAsync(Id);
                await alertService.DisplayAlert("Success", "Cloth deleted successfully", "OK");
                parentViewModel.RemoveCloth(Id);
            }
            catch
            {
                await alertService.DisplayAlert("Cannot Delete", "Cloth can not deleted", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task ShowOptions()
    {
        var optionsSheet = new ItemOptionsSheet();
        
        optionsSheet.EditRequested += async (s, e) => await Edit();
        optionsSheet.DeleteRequested += async (s, e) => await Delete();
        
        await Shell.Current.Navigation.PushModalAsync(optionsSheet);
    }
}