using CommunityToolkit.Mvvm.ComponentModel;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Windows.Input;

namespace OMS.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    public LoginViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

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

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Please enter username and password", "OK");
            SignInButtonText = "Sign In";
            return;
        }

        try
        {
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
                    await Shell.Current.GoToAsync("//Dashboard");
                    SignInButtonText = "Sign In";
                    return;
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid admin credentials", "OK");
                    SignInButtonText = "Sign In";
                    return;
                }
            }
            else
            {
                var employees = await _dataService.GetEmployeesAsync();
                var employee = employees.FirstOrDefault(e => e.Username == Username && e.Password == Password);

                if (employee == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid username or password", "OK");
                    SignInButtonText = "Sign In";
                    return;
                }

                var loggedInUser = new User(
                    Id: employee.Id,
                    Name: employee.Name,
                    Role: UserRole.Maker
                );

                App.CurrentUser = loggedInUser;

                var page = new MakerWorkspacePage
                {
                    BindingContext = new MakerWorkspaceViewModel(_dataService, loggedInUser.Id, loggedInUser.Name)
                };
                await Shell.Current.Navigation.PushAsync(page);
            }
        }
        catch
        {
            await Application.Current.MainPage.DisplayAlert("Error", "An error occurred during login. Please try again.", "OK");
        }
        finally
        {
            SignInButtonText = "Sign In";
        }
    }
}
