using OMS.Pages;

namespace OMS;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("Login", typeof(LoginPage));
        Routing.RegisterRoute("Dashboard", typeof(DashboardPage));
        Routing.RegisterRoute("ClothInventory", typeof(ClothInventoryPage));
        Routing.RegisterRoute("DressOrders", typeof(DressOrdersPage));
        Routing.RegisterRoute("MakerWorkspace", typeof(MakerWorkspacePage));
        Routing.RegisterRoute("MakerWorkspacePage", typeof(MakerWorkspacePage));
        Routing.RegisterRoute("AddMaker", typeof(AddMakerDialog));
    }
}
