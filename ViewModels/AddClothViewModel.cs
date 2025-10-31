using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Services;

namespace OMS.ViewModels;

public partial class AddClothViewModel(IDataService dataService) : ObservableObject
{
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

        var cloth = new Cloth
        {
            Name = Name,
            Color = Color,
            PricePerMeter = priceValue,
            TotalMeters = metersValue,
            RemainingMeters = metersValue,
            AddedDate = DateTime.Now
        };

        await dataService.AddClothAsync(cloth);
        await Close();
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
