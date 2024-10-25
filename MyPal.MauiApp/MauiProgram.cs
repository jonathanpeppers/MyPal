using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using MyPal.ClassLibrary;
using Plugin.Maui.Audio;

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
#elif ANDROID
        builder.AddAudio();
        builder.Services
            .AddSingleton<IMicrophone, AndroidMicrophone>()
            .AddSingleton<ISpeaker, MauiSpeaker>();
#else
        builder.AddAudio(
            playbackOptions =>
            {
                playbackOptions.Category = AVFoundation.AVAudioSessionCategory.Playback;
            },
            recordingOptions =>
            {
                recordingOptions.Category = AVFoundation.AVAudioSessionCategory.Record;
                recordingOptions.Mode = AVFoundation.AVAudioSessionMode.Default;
                recordingOptions.CategoryOptions = AVFoundation.AVAudioSessionCategoryOptions.MixWithOthers;
            });

        builder.Services
            .AddSingleton<IMicrophone, MauiMicrophone>()
            .AddSingleton<ISpeaker, MauiSpeaker>();
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
