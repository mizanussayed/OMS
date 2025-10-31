using OMS.ViewModels;

namespace OMS.Pages;

public partial class DressOrdersPage : ContentPage
{
    private readonly DressOrdersViewModel _viewModel;

    public DressOrdersPage(DressOrdersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        SetStatusBarColor();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        SetStatusBarColor();

        // Reload data when page appears to reflect any changes
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
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