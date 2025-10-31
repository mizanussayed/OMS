using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class ClothPickerItem : ObservableObject
{
    public Cloth Cloth { get; }
    public string DisplayText => $"{Cloth.Name} - {Cloth.Color}";

    public ClothPickerItem(Cloth cloth)
    {
        Cloth = cloth;
    }
}

public partial class NewOrderViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly IAlert _alertService;

    [ObservableProperty]
    private string customerName = string.Empty;

    [ObservableProperty]
    private string dressType = string.Empty;

    [ObservableProperty]
    private string metersUsed = string.Empty;

    [ObservableProperty]
    private string customerNameError = string.Empty;

    [ObservableProperty]
    private string dressTypeError = string.Empty;

    [ObservableProperty]
    private string clothError = string.Empty;

    [ObservableProperty]
    private string metersError = string.Empty;

    [ObservableProperty]
    private string metersHelper = string.Empty;

    [ObservableProperty]
    private bool hasClothError;

    [ObservableProperty]
    private ObservableCollection<ClothPickerItem> cloths = new();

    [ObservableProperty]
    private ClothPickerItem? selectedCloth;

    [ObservableProperty]
    private bool hasSelectedCloth;

    [ObservableProperty]
    private string selectedClothInfo = string.Empty;

    [ObservableProperty]
    private Color selectedClothColor = Colors.Gray;

    [ObservableProperty]
    private string selectedClothPrice = string.Empty;

    [ObservableProperty]
    private string selectedClothStock = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> makers = new() { "Maker 1", "Maker 2", "Maker 3" };

    [ObservableProperty]
    private int? selectedMaker;

    [ObservableProperty]
    private bool hasCostPreview;

    [ObservableProperty]
    private double estimatedCost;

    public NewOrderViewModel(IDataService dataService, IAlert alertService)
    {
        _dataService = dataService;
        _alertService = alertService;
        LoadCloths();
    }

    private async void LoadCloths()
    {
        var clothsList = await _dataService.GetClothsAsync();
        Cloths.Clear();
        foreach (var cloth in clothsList)
        {
            Cloths.Add(new ClothPickerItem(cloth));
        }
    }

    partial void OnCustomerNameChanged(string value)
    {
        CustomerNameError = string.Empty;
    }

    partial void OnDressTypeChanged(string value)
    {
        DressTypeError = string.Empty;
    }

    partial void OnSelectedClothChanged(ClothPickerItem? value)
    {
        ClothError = string.Empty;
        HasClothError = false;
        HasSelectedCloth = value != null;

        if (value != null)
        {
            SelectedClothInfo = $"{value.Cloth.Name} - {value.Cloth.Color}";
            SelectedClothColor = GetColorFromName(value.Cloth.Color);
            SelectedClothPrice = value.Cloth.PricePerMeter.ToString();
            SelectedClothStock = value.Cloth.RemainingMeters.ToString();
        }

        UpdateCostPreview();
    }

    partial void OnMetersUsedChanged(string value)
    {
        MetersError = string.Empty;
        MetersHelper = string.Empty;

        if (SelectedCloth != null && double.TryParse(value, out var meters))
        {
            if (meters > SelectedCloth.Cloth.RemainingMeters)
            {
                MetersHelper = $"⚠️ Only {SelectedCloth.Cloth.RemainingMeters}m available";
            }
        }

        UpdateCostPreview();
    }

    private void UpdateCostPreview()
    {
        if (SelectedCloth != null && double.TryParse(MetersUsed, out var meters))
        {
            EstimatedCost = meters * SelectedCloth.Cloth.PricePerMeter;
            HasCostPreview = true;
        }
        else
        {
            HasCostPreview = false;
        }
    }

    private Color GetColorFromName(string colorName)
    {
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
    private async Task CreateOrder()
    {
        // Validate
        var isValid = true;

        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            CustomerNameError = "Customer name is required";
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(DressType))
        {
            DressTypeError = "Dress type is required";
            isValid = false;
        }

        if (SelectedCloth == null)
        {
            ClothError = "Please select a cloth";
            HasClothError = true;
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(MetersUsed) || !double.TryParse(MetersUsed, out var meters) || meters <= 0)
        {
            MetersError = "Valid meters is required";
            isValid = false;
        }
        else if (SelectedCloth != null && meters > SelectedCloth.Cloth.RemainingMeters)
        {
            MetersError = $"Not enough stock. Only {SelectedCloth.Cloth.RemainingMeters}m available";
            isValid = false;
        }

        if (!isValid) return;

        double.TryParse(MetersUsed, out var metersValue);

        // Create new order
        var order = new DressOrder
        {
            CustomerName = CustomerName,
            MobileNumber = "", // Assuming optional
            DressType = DressType,
            ClothId = SelectedCloth!.Cloth.Id,
            MetersUsed = metersValue,
            Status = DressOrderStatus.Pending,
            AssignedTo = SelectedMaker??0,
            OrderDate = DateTime.Now
        };

        await _dataService.AddOrderAsync(order);

        // Close dialog
        await Close();
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
