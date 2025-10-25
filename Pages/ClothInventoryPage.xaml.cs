using OMS.ViewModels;

namespace OMS.Pages;

public partial class ClothInventoryPage : ContentPage
{
    public ClothInventoryPage(ClothInventoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}