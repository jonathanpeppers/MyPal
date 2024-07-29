using CommunityToolkit.Maui.Core.Primitives;

namespace MyPal.MauiApp;

public partial class MainPage : ContentPage
{
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
}
