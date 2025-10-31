using OMS.Pages;
using OMS.Services;
using OMS.ViewModels;

namespace OMS
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services
            builder.Services.AddSingleton<IDataService, FirebaseDataService>();
            builder.Services.AddSingleton<IAlert, AlertService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<ClothInventoryViewModel>();
            builder.Services.AddTransient<DressOrdersViewModel>();
            builder.Services.AddTransient<AddClothViewModel>();
            builder.Services.AddTransient<NewOrderViewModel>();
            builder.Services.AddTransient<AddMakerViewModel>();

            // Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<ClothInventoryPage>();
            builder.Services.AddTransient<DressOrdersPage>();
            builder.Services.AddTransient<AddClothDialog>();
            builder.Services.AddTransient<NewOrderDialog>();
            builder.Services.AddTransient<MakerWorkspacePage>();
            builder.Services.AddTransient<AddMakerDialog>();

            return builder.Build();
        }
    }
}
