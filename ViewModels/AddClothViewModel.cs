using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Services;

namespace OMS.ViewModels;

public partial class AddClothViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly Cloth? _existingCloth;
    private readonly bool _isEditMode;

    [ObservableProperty]
    private string pageTitle = "Add New Cloth";

    [ObservableProperty]
    private string submitButtonText = "Add to Inventory";

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string color = string.Empty;

    [ObservableProperty]
    private string pricePerMeter = string.Empty;

    [ObservableProperty]
    private string totalMeters = string.Empty;

    [ObservableProperty]
    private string nameError = string.Empty;

    [ObservableProperty]
    private string colorError = string.Empty;

    [ObservableProperty]
    private string priceError = string.Empty;

    [ObservableProperty]
    private string metersError = string.Empty;

    [ObservableProperty]
    private bool hasPreview;

    [ObservableProperty]
    private string previewText = string.Empty;

    [ObservableProperty]
    private Color previewColor = Colors.Gray;

    [ObservableProperty]
    private string previewPrice = string.Empty;

    [ObservableProperty]
    private string previewMeters = string.Empty;

    [ObservableProperty]
    private string previewValue = string.Empty;

    // Constructor for Add mode
    public AddClothViewModel(IDataService dataService)
    {
        _dataService = dataService;
        _isEditMode = false;
    }

    // Constructor for Edit mode
    public AddClothViewModel(IDataService dataService, Cloth cloth)
    {
        _dataService = dataService;
        _existingCloth = cloth;
        _isEditMode = true;

        PageTitle = "Edit Cloth";
        SubmitButtonText = "Update Cloth";

        // Pre-populate fields
        Name = cloth.Name;
        Color = cloth.Color;
        PricePerMeter = cloth.PricePerMeter.ToString();
        TotalMeters = cloth.TotalMeters.ToString();
    }

    partial void OnNameChanged(string value)
    {
        NameError = string.Empty;
        UpdatePreview();
    }

    partial void OnColorChanged(string value)
    {
        ColorError = string.Empty;
        UpdatePreview();
    }

    partial void OnPricePerMeterChanged(string value)
    {
        PriceError = string.Empty;
        UpdatePreview();
    }

    partial void OnTotalMetersChanged(string value)
    {
        MetersError = string.Empty;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        HasPreview = !string.IsNullOrWhiteSpace(Name) || 
                     !string.IsNullOrWhiteSpace(Color) || 
                     !string.IsNullOrWhiteSpace(PricePerMeter) || 
                     !string.IsNullOrWhiteSpace(TotalMeters);

        if (!HasPreview) return;

        PreviewText = $"{Name} - {Color}";
        PreviewColor = GetColorFromName(Color);
        PreviewPrice = PricePerMeter;
        PreviewMeters = TotalMeters;

        if (double.TryParse(PricePerMeter, out var price) && 
            double.TryParse(TotalMeters, out var meters))
        {
            PreviewValue = (price * meters).ToString("N0");
        }
        else
        {
            PreviewValue = "0";
        }
    }

    private static Color GetColorFromName(string colorName)
    {
        if (string.IsNullOrWhiteSpace(colorName)) return Colors.Gray;

        return colorName.ToLower() switch
        {
            "red" => Colors.Red,
            "blue" => Colors.Blue,
            "green" => Colors.Green,
            "yellow" => Colors.Yellow,
            "orange" => Colors.Orange,
            "purple" => Colors.Purple,
            "pink" => Colors.Pink,
            "brown" => Colors.Brown,
            "black" => Colors.Black,
            "white" => Colors.White,
            "gray" or "grey" => Colors.Gray,
            _ => Colors.Gray
        };
    }

    [RelayCommand]
    private async Task AddCloth()
    {
        var isValid = true;

        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Cloth name is required";
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(Color))
        {
            ColorError = "Color is required";
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(PricePerMeter) || !double.TryParse(PricePerMeter, out var price) || price <= 0)
        {
            PriceError = "Valid price is required";
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(TotalMeters) || !double.TryParse(TotalMeters, out var meters) || meters <= 0)
        {
            MetersError = "Valid meters is required";
            isValid = false;
        }

        if (!isValid) return;

        double.TryParse(PricePerMeter, out double priceValue);
        double.TryParse(TotalMeters, out double metersValue);

        if (_isEditMode && _existingCloth != null)
        {
            // Edit mode - update existing cloth
            var updatedCloth = new Cloth
            {
                Id = _existingCloth.Id,
                UniqueCode = _existingCloth.UniqueCode,
                Name = Name,
                Color = Color,
                PricePerMeter = priceValue,
                TotalMeters = metersValue,
                RemainingMeters = _existingCloth.RemainingMeters + (metersValue - _existingCloth.TotalMeters),
                AddedDate = _existingCloth.AddedDate
            };

            await _dataService.UpdateClothAsync(updatedCloth);
        }
        else
        {
            // Add mode - create new cloth
            var cloth = new Cloth
            {
                Name = Name,
                Color = Color,
                PricePerMeter = priceValue,
                TotalMeters = metersValue,
                RemainingMeters = metersValue,
                AddedDate = DateTime.Now
            };

            await _dataService.AddClothAsync(cloth);
        }

        await Close();
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
