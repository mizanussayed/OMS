using OMS.Models;
using OMS.Pages;

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
        if (CurrentUser == null)
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
                // Fallback, but should not happen
                return new Window(new AppShell());
            }
        }
    }

    public static void SwitchToAppShell()
    {
        if (Current?.Windows.Count > 0)
        {
            Current.Windows[0].Page = new AppShell();
        }
    }
}