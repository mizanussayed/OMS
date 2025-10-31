using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Services;

namespace OMS.ViewModels;

public partial class AddMakerViewModel(IDataService dataService) : ObservableObject
{
    private readonly IDataService _dataService = dataService;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [RelayCommand]
    private async Task AddMaker()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Please fill all fields", "OK");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Passwords do not match", "OK");
            return;
        }

        var employee = new Employee
        {
            Name = Name,
            Username = Username,
            Password = Password,
            MobileNumber = "" // Assuming mobile is optional or add a field
        };

        await _dataService.AddEmployeeAsync(employee);

        await Application.Current.MainPage.DisplayAlert("Success", $"Maker {Name} added successfully", "OK");

        Name = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;

        await Shell.Current.GoToAsync("..");
    }
}