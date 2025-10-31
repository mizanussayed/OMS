using OMS.ViewModels;

namespace OMS.Pages;

public partial class AddMakerPage : ContentPage
{
    public AddMakerPage(AddMakerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}