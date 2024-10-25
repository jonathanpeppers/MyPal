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

    readonly QueuedStream _stream = new();
    MediaRecorder? _recorder;
    string? _currentFile;

    public async void Start()
    {
        await CheckPermission();
        StartInternal();
    }

    void StartInternal()
    {
        _currentFile = Path.GetTempFileName();
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            _recorder = new(Android.App.Application.Context);
        }
        else
        {
            _recorder = new();
        }
        _recorder.SetAudioSource(AudioSource.Mic);
        _recorder.SetOutputFormat((OutputFormat)(int)Encoding.Pcm16bit);
        _recorder.SetAudioEncoder(AudioEncoder.Aac);
        _recorder.SetAudioSamplingRate(24000);
        _recorder.SetAudioEncodingBitRate(2 * 8);
        _recorder.SetAudioChannels(1);
        _recorder.SetOnInfoListener(this);
        _recorder.SetOnErrorListener(this);
        _recorder.SetMaxDuration(max_duration_ms: 3000);
        _recorder.SetOutputFile(_currentFile);
        _recorder.Prepare();
        _recorder.Start();
    }

    public System.IO.Stream GetAudio() => _stream;

    public void OnInfo(MediaRecorder? mr, [GeneratedEnum] MediaRecorderInfo what, int extra)
    {
        Console.WriteLine($"{nameof(AndroidMicrophone)}, {nameof(OnInfo)}: {what}");

        ArgumentNullException.ThrowIfNull(_recorder);
        ArgumentNullException.ThrowIfNull(_currentFile);

        if (what == MediaRecorderInfo.MaxFilesizeApproaching ||
            what == MediaRecorderInfo.MaxFilesizeReached ||
            what == MediaRecorderInfo.MaxDurationReached)
        {
            // Stop & enqueue the file
            _recorder.Stop();
            _recorder.Release();
            using var file = File.OpenRead(_currentFile);
            _stream.Enqueue(file);

            // Start the next file
            StartInternal();
        }
    }

    public void OnError(MediaRecorder? mr, [GeneratedEnum] MediaRecorderError what, int extra)
    {
        Console.WriteLine($"{nameof(AndroidMicrophone)}, {nameof(OnError)}: {what}");
    }
}
