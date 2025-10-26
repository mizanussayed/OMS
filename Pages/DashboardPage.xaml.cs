using OMS.ViewModels;

namespace OMS.Pages;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        
        // Set status bar color to match gradient header
        SetStatusBarColor();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetStatusBarColor();
    }

    private void SetStatusBarColor()
    {
        if (Platform.CurrentActivity?.Window != null)
        {
            var window = Platform.CurrentActivity.Window;
#pragma warning disable CA1422
            window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#9333ea"));
            window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#ffffff"));
#pragma warning restore CA1422
        }
    }
}