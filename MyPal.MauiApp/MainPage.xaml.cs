using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using MyPal.ClassLibrary;

namespace MyPal.MauiApp;

public partial class MainPage : ContentPage, ICharacter
{
    readonly TelemetryClient _telemetry;
    readonly MyPalWebClient _client = new();
    readonly IMicrophone _microphone;
    readonly ISpeaker _speaker;
    readonly CancellationTokenSource _cancelAwake = new();
    readonly CancellationTokenSource _source = new();
    bool _insult = true;

    public MainPage(TelemetryClient telemetry, IMicrophone microphone, ISpeaker speaker)
    {
        _telemetry = telemetry;
        _microphone = microphone;
        _speaker = speaker;
        InitializeComponent();
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await Task.Delay(6500, _cancelAwake.Token);
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

        _source.Cancel();
    }

    async void Insult_Clicked(object sender, EventArgs e)
    {
        _insult = true;
        await StartConversation();
    }

    async void Compliment_Clicked(object sender, EventArgs e)
    {
        _insult = false;
        await StartConversation();
    }

    async Task StartConversation()
    {
        _insultButton.IsVisible =
            _complimentButton.IsVisible = false;
        await _client.StartConversation(_microphone, _speaker, character: this, _insult);
    }

    public async void StartTalking()
    {
        await Dispatcher.DispatchAsync(() =>
        {
            _image.Source = ImageSource.FromFile("koala_talk.gif");
        });
    }

    public async void Idle()
    {
        await Dispatcher.DispatchAsync(() =>
        {
            _image.Source = ImageSource.FromFile("koala_idle.gif");
        });
    }
}
