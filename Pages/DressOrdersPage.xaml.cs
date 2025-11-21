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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadDataCommand.ExecuteAsync(null);
    }
}