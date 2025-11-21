using OMS.ViewModels;

namespace OMS.Pages;

public partial class EmployeesPage : ContentPage
{
    private readonly EmployeesViewModel _viewModel;
    public EmployeesPage(EmployeesViewModel viewModel)
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
