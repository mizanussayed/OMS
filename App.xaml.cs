using OMS.Models;
using OMS.Pages;
using OMS.Services;
using OMS.ViewModels;

namespace OMS;

public partial class App : Application
{
    public static User? CurrentUser { get; set; }
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        if (CurrentUser is not null)
        {
            return new Window(new AppShell());
        }
        else
        {
            var loginPage = Handler?.MauiContext?.Services.GetService<LoginPage>();
            if (loginPage != null)
            {
                return new Window(new NavigationPage(loginPage));
            }
            else
            {
                return new Window(new AppShell());
            }
        }
    }

    public static void SwitchToAppShell()
    {
        if (Current?.Windows.Count > 0)
        {
            if (CurrentUser?.Role == UserRole.Admin)
            {
                Current.Windows[0].Page = new AppShell();
            }
            else if (CurrentUser?.Role == UserRole.Maker)
            {
                var services = Current.Handler?.MauiContext?.Services;
                if (services is not null)
                {
                    var dataService = services.GetService<IDataService>();
                    var alertService = services.GetService<IAlert>();
                    if (dataService is not null && alertService is not null)
                    {
                        var viewModel = new MakerWorkspaceViewModel(dataService, alertService, CurrentUser.Id, CurrentUser.Name);
                        var makerPage = new MakerWorkspacePage(viewModel);
                        Current.Windows[0].Page = new NavigationPage(makerPage);
                    }
                }
            }
        }
    }
}