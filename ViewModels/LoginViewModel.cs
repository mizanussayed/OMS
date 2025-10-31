using CommunityToolkit.Mvvm.ComponentModel;
using OMS.Models;
using OMS.Services;
using System.Windows.Input;

namespace OMS.ViewModels;

public partial class LoginViewModel(IDataService dataService, IAlert alertService) : ObservableObject
{
    private bool _isAdminSelected = true;
    private bool _isMakerSelected;
    private string _username = string.Empty;
    private string _password = string.Empty;

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool IsAdminSelected
    {
        get => _isAdminSelected;
        set => SetProperty(ref _isAdminSelected, value);
    }

    public bool IsMakerSelected
    {
        get => _isMakerSelected;
        set => SetProperty(ref _isMakerSelected, value);
    }

    private string _signInButtonText = "Sign In";
    public string SignInButtonText
    {
        get => _signInButtonText;
        set => SetProperty(ref _signInButtonText, value);
    }

    public ICommand SelectRoleCommand => new Command<string>(SelectRole);
    public ICommand LoginCommand => new Command(async () => await Login());

    private void SelectRole(string role)
    {
        if (role == "admin")
        {
            IsAdminSelected = true;
            IsMakerSelected = false;
        }
        else if (role == "maker")
        {
            IsAdminSelected = false;
            IsMakerSelected = true;
        }
    }

    private async Task Login()
    {
        SignInButtonText = "Signing In...";

        try
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await alertService.DisplayAlert("Error", "Please enter username and password", "OK");
                return;
            }

            if (IsAdminSelected)
            {
                if (Username == "admin" && Password == "admin")
                {
                    var adminUser = new User(
                        Id: 1,
                        Name: "Admin User",
                        Role: UserRole.Admin
                    );
                    App.CurrentUser = adminUser;
                    
                    App.SwitchToAppShell();
                    return;
                }
                else
                {
                    await alertService.DisplayAlert("Error", "Invalid admin credentials", "OK");
                    return;
                }
            }
            else
            {
                var employees = await dataService.GetEmployeesAsync();
                var employee = employees.FirstOrDefault(e => e.Username == Username && e.Password == Password);

                if (employee == null)
                {
                    await alertService.DisplayAlert("Error", "Invalid username or password", "OK");
                    return;
                }

                var loggedInUser = new User(
                    Id: employee.Id,
                    Name: employee.Name,
                    Role: UserRole.Maker
                );

                App.CurrentUser = loggedInUser;

                var parameters = new Dictionary<string, object>
                {
                    { "userId", loggedInUser.Id },
                    { "userName", loggedInUser.Name }
                };

                App.SwitchToAppShell();
                await Task.Delay(100);
                await Shell.Current.GoToAsync("//MakerWorkspacePage", parameters);
            }
        }
        catch
        {
            await alertService.DisplayAlert("Error", "An error occurred during login. Please try again.", "OK");
        }
        finally
        {
            SignInButtonText = "Sign In";
        }
    }
}
