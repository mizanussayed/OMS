using OMS.ViewModels;

namespace OMS.Pages;

public partial class ClothInventoryPage : ContentPage
{
    private readonly ClothInventoryViewModel _viewModel;

    public ClothInventoryPage(ClothInventoryViewModel viewModel)
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