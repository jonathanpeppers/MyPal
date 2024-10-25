using Android.Media;
using Android.Runtime;
using MyPal.ClassLibrary;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace MyPal.MauiApp;

class AndroidMicrophone : Java.Lang.Object, IMicrophone, MediaRecorder.IOnInfoListener, MediaRecorder.IOnErrorListener
{
    async Task CheckPermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Microphone>();
        if (status == PermissionStatus.Denied && OperatingSystem.IsIOS())
        {
            throw new Exception("Microphone permission is required!");
        }
        else if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Microphone>();

            if (status != PermissionStatus.Granted)
                throw new Exception("Microphone permission is required!");
        }
    }

    readonly MediaRecorder _recorder;
    readonly QueuedStream _stream = new();
    string _currentFile = Path.GetTempFileName();

    public AndroidMicrophone()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            _recorder = new(Android.App.Application.Context);
        }
        else
        {
            _recorder = new();
        }
        _recorder.SetAudioSource(AudioSource.Mic);
        _recorder.SetOutputFormat(OutputFormat.Default);
        _recorder.SetAudioEncoder(AudioEncoder.Default);
        _recorder.SetOnInfoListener(this);
        _recorder.SetOnErrorListener(this);
        _recorder.SetMaxDuration(max_duration_ms: 3000);
        _recorder.SetOutputFile(_currentFile);
        _recorder.Prepare();
    }

    public async void Start()
    {
        await CheckPermission();

        _recorder.Start();
    }

    public System.IO.Stream GetAudio() => _stream;

    public void OnInfo(MediaRecorder? mr, [GeneratedEnum] MediaRecorderInfo what, int extra)
    {
        Console.WriteLine($"{nameof(AndroidMicrophone)}, {nameof(OnInfo)}: {what}");

        if (what == MediaRecorderInfo.MaxFilesizeApproaching ||
            what == MediaRecorderInfo.MaxFilesizeReached ||
            what == MediaRecorderInfo.MaxDurationReached)
        {
            // Stop & enqueue the file
            _recorder.Stop();
            using var file = File.OpenRead(_currentFile);
            _stream.Enqueue(file);

            // Start the next file
            _recorder.SetOutputFile(_currentFile = Path.GetTempFileName());
            _recorder.Prepare();
            _recorder.Start();
        }
    }

    public void OnError(MediaRecorder? mr, [GeneratedEnum] MediaRecorderError what, int extra)
    {
        Console.WriteLine($"{nameof(AndroidMicrophone)}, {nameof(OnError)}: {what}");
    }
}
