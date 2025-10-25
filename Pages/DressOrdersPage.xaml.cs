using OMS.ViewModels;

namespace OMS.Pages;

public partial class DressOrdersPage : ContentPage
{
    public DressOrdersPage(DressOrdersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}