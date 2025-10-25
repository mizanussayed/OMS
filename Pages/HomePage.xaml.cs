using OMS.ViewModels;

namespace OMS.Pages;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
        BindingContext = new HomeViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HomeViewModel vm)
            await vm.LoadDataCommand.ExecuteAsync(null);
    }
}
