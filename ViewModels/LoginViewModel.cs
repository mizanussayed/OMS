using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OMS.Models;
using OMS.Pages;
using OMS.Services;
using System.Collections.ObjectModel;
namespace OMS.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool isAdminSelected = true;

    [ObservableProperty]
    private bool isMakerSelected = false;

    [ObservableProperty]
    private string selectedMaker = "Rajesh Kumar";

    public LoginViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public ObservableCollection<string> Makers { get; } = new()
    {
        "Rajesh Kumar",
        "Amit Patel",
        "Suresh Singh"
    };

    public string SignInButtonText => IsAdminSelected ? "→  Sign In as Admin" : "→  Sign In as Maker";

    // Admin role styling
    public Color AdminBorderColor => IsAdminSelected 
        ? Microsoft.Maui.Graphics.Color.FromArgb("#9333ea") 
        : Microsoft.Maui.Graphics.Color.FromArgb("#e5e7eb");
    
    public Color AdminBackgroundColor => IsAdminSelected 
        ? Microsoft.Maui.Graphics.Color.FromArgb("#f5f3ff") 
        : Microsoft.Maui.Graphics.Color.FromArgb("#ffffff");

    // Maker role styling
    public Color MakerBorderColor => IsMakerSelected 
        ? Microsoft.Maui.Graphics.Color.FromArgb("#9333ea") 
        : Microsoft.Maui.Graphics.Color.FromArgb("#e5e7eb");
    
    public Color MakerBackgroundColor => IsMakerSelected 
        ? Microsoft.Maui.Graphics.Color.FromArgb("#f5f3ff") 
        : Microsoft.Maui.Graphics.Color.FromArgb("#ffffff");

    partial void OnIsAdminSelectedChanged(bool value)
    {
        if (value)
        {
            IsMakerSelected = false;
        }
        OnPropertyChanged(nameof(SignInButtonText));
        OnPropertyChanged(nameof(AdminBorderColor));
        OnPropertyChanged(nameof(AdminBackgroundColor));
        OnPropertyChanged(nameof(MakerBorderColor));
        OnPropertyChanged(nameof(MakerBackgroundColor));
    }

    partial void OnIsMakerSelectedChanged(bool value)
    {
        if (value)
        {
            IsAdminSelected = false;
        }
        OnPropertyChanged(nameof(SignInButtonText));
        OnPropertyChanged(nameof(AdminBorderColor));
        OnPropertyChanged(nameof(AdminBackgroundColor));
        OnPropertyChanged(nameof(MakerBorderColor));
        OnPropertyChanged(nameof(MakerBackgroundColor));
    }

    [RelayCommand]
    private void SelectRole(string role)
    {
        if (role == "admin")
        {
            IsAdminSelected = true;
        }
        else if (role == "maker")
        {
            IsMakerSelected = true;
        }
    }

    [RelayCommand]
    private async Task Login()
    {
        var user = new User(
            Id: IsAdminSelected ? "admin-1" : GetMakerId(SelectedMaker),
            Name: IsAdminSelected ? "Admin User" : SelectedMaker,
            Role: IsAdminSelected ? UserRole.Admin : UserRole.Maker
        );

        App.CurrentUser = user;

        if (user.Role == UserRole.Maker)
        {
            var page = new MakerWorkspacePage
            {
                BindingContext = new MakerWorkspaceViewModel(_dataService, user.Id, user.Name)
            };
            if (Shell.Current != null)
            {
                await Shell.Current.Navigation.PushAsync(page);
            }
        }
        else
        {
            App.SwitchToAppShell();
        }
    }

    private string GetMakerId(string makerName)
    {
        return makerName switch
        {
            "Rajesh Kumar" => "maker-1",
            "Amit Patel" => "maker-2",
            "Suresh Singh" => "maker-3",
            _ => "maker-1"
        };
    }
}
