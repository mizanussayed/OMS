using OMS.ViewModels;

namespace OMS.Pages;

public partial class AddMakerDialog : ContentPage
{
    public AddMakerDialog(AddMakerViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}