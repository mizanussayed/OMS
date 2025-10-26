using Microsoft.Extensions.Logging;
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
            builder.Services.AddSingleton<IDataService, MockDataService>();

            // ViewModels
            builder.Services.AddScoped<LoginViewModel>(sp => new LoginViewModel(sp.GetRequiredService<IDataService>()));
            builder.Services.AddScoped<DashboardViewModel>();
            builder.Services.AddScoped<ClothInventoryViewModel>();
            builder.Services.AddScoped<DressOrdersViewModel>();
            builder.Services.AddScoped<HomeViewModel>();
            builder.Services.AddScoped<AddClothViewModel>();
            builder.Services.AddScoped<NewOrderViewModel>();

            // Pages
            builder.Services.AddScoped<LoginPage>();
            builder.Services.AddScoped<DashboardPage>();
            builder.Services.AddScoped<ClothInventoryPage>();
            builder.Services.AddScoped<DressOrdersPage>();
            builder.Services.AddScoped<HomePage>();
            builder.Services.AddScoped<AddClothDialog>();
            builder.Services.AddScoped<NewOrderDialog>();
            builder.Services.AddScoped<MakerWorkspacePage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
