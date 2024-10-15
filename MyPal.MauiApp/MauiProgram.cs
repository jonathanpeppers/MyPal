using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using MyPal.ClassLibrary;

namespace MyPal.MauiApp;

public static class MauiProgram
{
    public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
    {
        var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services
            .AddSingleton<MainPage>()
            .AddSingleton<TelemetryClient>();

#if WINDOWS
        builder.Services
            .AddSingleton<IMicrophone, NAudioMicrophone>()
            .AddSingleton<ISpeaker, NAudioSpeaker>();
#endif

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
