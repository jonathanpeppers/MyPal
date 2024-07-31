using CommunityToolkit.Maui;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

namespace MyPal.MauiApp;

public static class MauiProgram
{
    public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
    {
        var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitCamera()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services
            .AddSingleton<MainPage>()
            .AddSingleton<TelemetryClient>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Logging.AddApplicationInsights(
            configureTelemetryConfiguration: (config) =>
                config.ConnectionString = AppContext.GetData("APPLICATIONINSIGHTS_CONNECTION_STRING") as string,
                configureApplicationInsightsLoggerOptions: (options) => { }
        );

        return builder.Build();
    }
}
