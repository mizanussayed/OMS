using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class ClothPickerItem(Cloth cloth) : ObservableObject
{
    public Cloth Cloth { get; } = cloth;
    public string DisplayText => $"{Cloth.Name} - {Cloth.Color}";
}

public partial class MakerPickerItem(Employee employee) : ObservableObject
{
    public Employee Employee { get; } = employee;
    public string DisplayText => Employee.Name;
}

public partial class NewOrderViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    private readonly IAlert _alertService;

    [ObservableProperty]
    private string customerName = string.Empty;

    [ObservableProperty]
    private string mobileNumber = string.Empty;

    [ObservableProperty]
    private string dressType = string.Empty;

    [ObservableProperty]
    private string metersUsed = string.Empty;

    [ObservableProperty]
    private string customerNameError = string.Empty;

    [ObservableProperty]
    private string mobileNumberError = string.Empty;

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
    private ObservableCollection<MakerPickerItem> makers = [];

    [ObservableProperty]
    private MakerPickerItem? selectedMaker;

    [ObservableProperty]
    private bool hasCostPreview;

    [ObservableProperty]
    private double estimatedCost;

    public NewOrderViewModel(IDataService dataService, IAlert alertService)
    {
        _dataService = dataService;
        _alertService = alertService;
        LoadData();
    }

    private async void LoadData()
    {
        var clothsList = await _dataService.GetClothsAsync();
        Cloths.Clear();
        foreach (var cloth in clothsList)
        {
            Cloths.Add(new ClothPickerItem(cloth));
        }

        var employeesList = await _dataService.GetEmployeesAsync();
        Makers.Clear();
        foreach (var employee in employeesList)
        {
            Makers.Add(new MakerPickerItem(employee));
        }
    }

    partial void OnCustomerNameChanged(string value)
    {
        CustomerNameError = string.Empty;
    }
    partial void OnMobileNumberChanged(string value)
    {
        MobileNumberError = string.Empty;
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

    private static Color GetColorFromName(string colorName)
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
        var isValid = true;

        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            CustomerNameError = "Customer name is required";
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(MobileNumber))
        {
            MobileNumberError = "Mobile number is required";
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

        try
        {
            var order = new DressOrder
            {
                CustomerName = CustomerName,
                MobileNumber = MobileNumber,
                DressType = DressType,
                ClothId = SelectedCloth!.Cloth.Id,
                MetersUsed = metersValue,
                Status = DressOrderStatus.Pending,
                AssignedTo = SelectedMaker?.Employee.Id ?? 0,
                OrderDate = DateTime.Now
            };

            await _dataService.AddOrderAsync(order);

            await _alertService.DisplayAlert("Success", "Order created successfully", "OK");

            // Close dialog
            await Close();
        }
        catch
        {
            await _alertService.DisplayAlert("Error", "Failed to create order. Please try again.", "OK");
        }
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}
