using OMS.Pages;

namespace OMS
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            // Register routes
            Routing.RegisterRoute("Login", typeof(LoginPage));
            Routing.RegisterRoute("Dashboard", typeof(DashboardPage));
            Routing.RegisterRoute("ClothInventory", typeof(ClothInventoryPage));
            Routing.RegisterRoute("DressOrders", typeof(DressOrdersPage));

            // Start at login page
            GoToAsync("//Login");
        }
    }
}
