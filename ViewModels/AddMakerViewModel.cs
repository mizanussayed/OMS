using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Services;

namespace OMS.ViewModels;

public partial class AddMakerViewModel(IDataService dataService, IAlert alertService) : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string mobileNumber = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string confirmPassword = string.Empty;

    [RelayCommand]
    private async Task AddMaker()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            await alertService.DisplayAlert("Error", "Please fill all required fields", "OK");
            return;
        }

        if (Password != ConfirmPassword)
        {
            await alertService.DisplayAlert("Error", "Passwords do not match", "OK");
            return;
        }

        var employee = new Employee
        {
            Name = Name,
            Username = Username,
            Password = Password,
            MobileNumber = MobileNumber,
        };

        await dataService.AddEmployeeAsync(employee);

        await alertService.DisplayAlert("Success", $"Maker {Name} added successfully", "OK");

        // Clear form
        Name = string.Empty;
        Username = string.Empty;
        MobileNumber = string.Empty;
        Password = string.Empty;
        ConfirmPassword = string.Empty;

        await Close();
    }

    [RelayCommand]
    private async Task Close()
    {
        await Shell.Current.Navigation.PopModalAsync();
    }
}