using OMS.ViewModels;

namespace OMS.Pages;

public partial class ClothInventoryPage : ContentPage
{
    public ClothInventoryPage(ClothInventoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        SetStatusBarColor();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SetStatusBarColor();
    }

    private void SetStatusBarColor()
    {
#if ANDROID
        if (Platform.CurrentActivity?.Window != null)
        {
            var window = Platform.CurrentActivity.Window;
#pragma warning disable CA1422
            window.SetStatusBarColor(Android.Graphics.Color.ParseColor("#9333ea"));
            window.SetNavigationBarColor(Android.Graphics.Color.ParseColor("#ffffff"));
#pragma warning restore CA1422
        }
#endif
    }
}