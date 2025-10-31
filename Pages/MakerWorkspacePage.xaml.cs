using OMS.Services;
using OMS.ViewModels;

namespace OMS.Pages;

public partial class MakerWorkspacePage : ContentPage
{
    private MakerWorkspaceViewModel? _viewModel;

    public MakerWorkspacePage(IDataService dataService)
    {
        InitializeComponent();
        _dataService = dataService;
    }

    private readonly IDataService _dataService;

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (BindingContext == null && App.CurrentUser != null)
        {
            _viewModel = new MakerWorkspaceViewModel(
                _dataService,
                App.CurrentUser.Id,
                App.CurrentUser.Name
            );
            BindingContext = _viewModel;
        }
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
