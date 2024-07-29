#if WINDOWS
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
#endif

namespace MyPal.MauiApp;

public partial class MainPage : ContentPage
{
    readonly CancellationTokenSource source = new();
#if WINDOWS
    MediaCapture? capture;
    ImageEncodingProperties encoding = ImageEncodingProperties.CreateJpeg();
#endif

    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Initialize();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        source.Cancel();
    }

    async void Initialize()
    {
#if WINDOWS
        capture = new MediaCapture();
        await capture.InitializeAsync();

        while (!source.IsCancellationRequested)
        {
            var stream = new InMemoryRandomAccessStream();
            await capture.CapturePhotoToStreamAsync(encoding, stream);
            stream.Seek(0);
            _image.Source = ImageSource.FromStream(() => stream.AsStream());
        }
#else
        await Task.Yield();
#endif
    }
}
