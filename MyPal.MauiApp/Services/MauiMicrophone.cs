using MyPal.ClassLibrary;
using Plugin.Maui.Audio;

namespace MyPal.MauiApp;

/// <summary>
/// Implements IMicrophone with Plugin.Maui.Audio
/// </summary>
class MauiMicrophone : IMicrophone
{
    readonly IAudioManager _audio;
    readonly IAudioRecorder _recorder;
    readonly QueuedStream _stream = new();

    public MauiMicrophone(IAudioManager audio)
    {
        _audio = audio;
        _recorder = _audio.CreateRecorder(new AudioRecorderOptions
        {
#if IOS || MACCATALYST
            Category = AVFoundation.AVAudioSessionCategory.PlayAndRecord
#endif
        });
    }

    async Task CheckPermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Microphone>();
        if (status == PermissionStatus.Denied && OperatingSystem.IsIOS())
        {
            throw new Exception("Microphone permission is required!");
        }
        else if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Microphone>();

            if (status != PermissionStatus.Granted)
                throw new Exception("Microphone permission is required!");
        }
    }

    /// <summary>
    /// NOTE: this is the best you can do with this plugin, record 3 seconds, queue, etc.
    /// </summary>
    public async void Start()
    {
        await CheckPermission();

        while (true)
        {
            await _recorder.StartAsync();
            await Task.Delay(3000);
            var audio = await _recorder.StopAsync();
            _stream.Enqueue(audio.GetAudioStream());
        }
    }

    public Stream GetAudio() => _stream;
}
