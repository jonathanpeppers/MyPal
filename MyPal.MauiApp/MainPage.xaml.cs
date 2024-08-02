using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using MyPal.ClassLibrary;
using System.Diagnostics;

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
        try
        {
            Dispatcher.Dispatch(_camera.StopCameraPreview);

            long length = e.Media.Length;
            telemetry.TrackMetric(new MetricTelemetry("Image.ByteCount", length));

            // Record time to first audio playback
            var sw = new Stopwatch();
            sw.Start();

            Task? task = null;
            using (telemetry.StartOperation<RequestTelemetry>("sendimage-streaming"))
            await foreach (var stream in client.SendImageStreaming(e.Media, "Fable"))
            {
                cancelAwake.Cancel();
                telemetry.TrackMetric(new MetricTelemetry("Audio.ByteCount", stream.Length));

                // Wait on the sound if still playing
                if (task is not null)
                    await task;

#if ANDROID || IOS
                await Dispatcher.DispatchAsync(() =>
                {
                    if (sw.IsRunning)
                    {
                        sw.Stop();
                        telemetry.TrackMetric(new MetricTelemetry("TimeToAudio", sw.ElapsedMilliseconds));
                    }

                    _image.Source = ImageSource.FromFile("koala_talk.gif");
                    _indicator.IsRunning = false;
                    task = Sound.Play(stream);
                });
#else
                //TODO: implement Sound.Play() on other platforms
                throw new NotImplementedException();
#endif
            }

            // Wait on the last sound
            if (task is not null)
                await task;
        }
        catch (Exception exc)
        {
            telemetry.TrackException(exc);
            Console.WriteLine("Error in OnMediaCaptured: {0}", exc);
            await Dispatcher.DispatchAsync(() => DisplayAlert("Oops!", "Failed to send image", "OK"));
        }

        await GoToIdle();
    }

    async Task GoToIdle()
    {
        await Dispatcher.DispatchAsync(async () =>
        {
            _image.Source = ImageSource.FromFile("koala_idle.gif");
            _indicator.IsRunning = false;
            _button.IsVisible = true;
            await _camera.StartCameraPreview(source.Token);
        });
    }

    void OnMediaCaptureFailed(object sender, MediaCaptureFailedEventArgs e) =>
        Dispatcher.DispatchAsync(async () =>
        {
            await DisplayAlert("Oops!", "Failed to capture image", "OK");
            await GoToIdle();
        });

    async void Button_Clicked(object sender, EventArgs e)
    {
        _indicator.IsRunning = true;
        _button.IsVisible = false;
        await _camera.CaptureImage(source.Token);
    }
}
