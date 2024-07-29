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
        var result = await client.SendImageAsync(e.Media);
        Dispatcher.Dispatch(() => DisplayAlert("Result", result, "OK"));
    }

    void OnMediaCaptureFailed(object sender, MediaCaptureFailedEventArgs e) =>
        DisplayAlert("Oops!", "Failed to capture image", "OK");

    async void Button_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            button.IsEnabled = false;
            await _camera.CaptureImage(source.Token);
            button.IsEnabled = false;
        }
    }
}
