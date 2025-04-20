using HMS.Data;
using HMS.Services;

namespace HMS
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
                });

            builder.Services.AddMauiBlazorWebView();
#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
#endif

            // Register services
            builder.Services.AddSingleton<DatabaseConnection>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddSingleton<UserStateService>();
            builder.Services.AddScoped<RecordService>();


            return builder.Build();
        }
    }
}