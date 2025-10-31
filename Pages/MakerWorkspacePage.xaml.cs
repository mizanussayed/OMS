using OMS.ViewModels;

namespace OMS.Pages;

public partial class MakerWorkspacePage : ContentPage
{
    private MakerWorkspaceViewModel? _viewModel;

    public MakerWorkspacePage(MakerWorkspaceViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel != null)
        {
            await _viewModel.LoadDataCommand.ExecuteAsync(null);
        }
    }
}
