using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using MyPal.ClassLibrary;

namespace MyPal.MauiApp;

public partial class MainPage : ContentPage
{
    readonly MyPalWebClient client = new();
    readonly CancellationTokenSource source = new();

    public MainPage()
    {
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
                return;
            }
        }

        // Then just grab first, if no front
        _camera.SelectedCamera = cameras.FirstOrDefault();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        source.Cancel();
    }

    async void OnMediaCaptured(object sender, MediaCapturedEventArgs e)
    {
        Dispatcher.Dispatch(_camera.StopCameraPreview);
        var result = await client.SendImageAsync(e.Media);
        Console.WriteLine(result);

        var stream = await client.TextToSpeechAsync(result, "Fable");

        await Dispatcher.DispatchAsync(async () =>
        {
            _indicator.IsRunning = false;
            _button.IsEnabled = true;
            await _camera.StartCameraPreview(source.Token);
#if ANDROID
            await Sound.Play(stream);
#else
            await DisplayAlert("Result", result, "OK");
#endif
        });
    }

    void OnMediaCaptureFailed(object sender, MediaCaptureFailedEventArgs e) =>
        Dispatcher.DispatchAsync(async () =>
        {
            _indicator.IsRunning = false;
            _button.IsEnabled = true;
            await DisplayAlert("Oops!", "Failed to capture image", "OK");
            await _camera.StartCameraPreview(source.Token);
        });

    void Button_Clicked(object sender, EventArgs e)
    {
        _indicator.IsRunning = true;
        _button.IsEnabled = false;
        _ = _camera.CaptureImage(source.Token);
    }
}
