using OMS.Models;

namespace OMS
{
    public partial class App : Application
    {
        public static User? CurrentUser { get; set; }

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}