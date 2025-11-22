using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;

namespace OMS.ViewModels;

public partial class EmployeesViewModel(IDataService dataService, IAlert alertService) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<EmployeeItemViewModel> employees = [];

    [ObservableProperty]
    private bool hasNoEmployees;

    [ObservableProperty]
    private bool isRefreshing;

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        IsRefreshing = true;
        
        try
        {
            var employeesList = await dataService.GetEmployeesAsync();
            var employeeViewModels = employeesList.Select(e => new EmployeeItemViewModel(e)).ToList();
            Employees = new ObservableCollection<EmployeeItemViewModel>(employeeViewModels);
            HasNoEmployees = !Employees.Any();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading employees: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task ShowAddEmployee()
    {
        var viewModel = new AddMakerViewModel(dataService, alertService);
        var dialog = new AddMakerDialog
        {
            BindingContext = viewModel
        };
        
        dialog.Disappearing += async (s, e) => 
        {
            await LoadDataAsync();
        };
        
        await Shell.Current.Navigation.PushModalAsync(dialog);
    }
}

public partial class EmployeeItemViewModel(Employee employee) : ObservableObject
{
    private readonly Employee _employee = employee;
    
    public int Id => _employee.Id;
    public string Name => _employee.Name;
    public string Username => _employee.Username;
    public string MobileNumber => _employee.MobileNumber;
    public string Password => _employee.Password;
    public string InitialsDisplay => GetInitials(_employee.Name);
    
    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "?";
        
        if (parts.Length == 1)
            return parts[0][..Math.Min(2, parts[0].Length)].ToUpper();
        
        return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpper();
    }
}
