using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using MyPal.ClassLibrary;

namespace MyPal.MauiApp;

public partial class MainPage : ContentPage
{
    readonly TelemetryClient telemetry;
    readonly MyPalWebClient client = new();
    readonly CancellationTokenSource cancelAwake = new();
    readonly CancellationTokenSource source = new();

    public MainPage(TelemetryClient telemetry)
    {
        this.telemetry = telemetry;
        InitializeComponent();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        var cameras = await _camera.GetAvailableCameras(source.Token);

        // Try to find a front camera
        foreach (var camera in cameras)
        {
            if (camera.Position == CameraPosition.Front)
            {
                _camera.SelectedCamera = camera;
                goto KoalaIdle;
            }
        }

        // Then just grab first, if no front
        _camera.SelectedCamera = cameras.FirstOrDefault();

    KoalaIdle:
        try
        {
            await Task.Delay(6500, cancelAwake.Token);
            _image.Source = ImageSource.FromFile("koala_idle.gif");
        }
        catch (TaskCanceledException)
        {
            // Expected
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        source.Cancel();
    }

    async void OnMediaCaptured(object sender, MediaCapturedEventArgs e)
    {
        Dispatcher.Dispatch(_camera.StopCameraPreview);

        string result;
        long length = e.Media.Length;
        using (telemetry.StartOperation<RequestTelemetry>("sendimage"))
        {
            telemetry.TrackMetric(new MetricTelemetry("Image.ByteCount", length));
            result = await client.SendImageAsync(e.Media);
            telemetry.TrackMetric(new MetricTelemetry("Result.CharacterCount", result.Length));
        }

        Stream stream;
        using (telemetry.StartOperation<RequestTelemetry>("tts"))
        {
            stream = await client.TextToSpeechAsync(result, "Fable");
            telemetry.TrackMetric(new MetricTelemetry("Audio.ByteCount", stream.Length));
        }

        await Dispatcher.DispatchAsync(async () =>
        {
            _indicator.IsRunning = false;
            cancelAwake.Cancel();

#if ANDROID || IOS
            _image.Source = ImageSource.FromFile("koala_talk.gif");
            await Sound.Play(stream);
#else
            //TODO: implement Sound.Play() on other platforms
            await DisplayAlert("Result", result, "OK");
#endif
            _image.Source = ImageSource.FromFile("koala_idle.gif");
            _button.IsVisible = true;
            await _camera.StartCameraPreview(source.Token);
        });
    }

    void OnMediaCaptureFailed(object sender, MediaCaptureFailedEventArgs e) =>
        Dispatcher.DispatchAsync(async () =>
        {
            _indicator.IsRunning = false;
            _button.IsVisible = true;
            await DisplayAlert("Oops!", "Failed to capture image", "OK");
            await _camera.StartCameraPreview(source.Token);
        });

    void Button_Clicked(object sender, EventArgs e)
    {
        _indicator.IsRunning = true;
        _button.IsVisible = false;
        _ = _camera.CaptureImage(source.Token);
    }
}
