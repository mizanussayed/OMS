using OMS.Models;
using OMS.Pages;
using OMS.ViewModels;
using OMS.Services;

namespace OMS
{
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
            // Check if user is already logged in
            if (CurrentUser != null)
            {
                return new Window(new AppShell());
            }
            else
            {
                // Start with Login page, then switch to AppShell after login
                var loginPage = Handler?.MauiContext?.Services.GetService<LoginPage>();
                return new Window(new NavigationPage(loginPage ?? new LoginPage(new LoginViewModel(new MockDataService()))));
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
}