using Android.Media;
using MyPal.ClassLibrary;
using static Microsoft.Maui.ApplicationModel.Permissions;

namespace MyPal.MauiApp;

class AndroidMicrophone : IMicrophone
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

    const int SAMPLING_RATE_IN_HZ = 24000;
    const ChannelIn CHANNELS = ChannelIn.Mono;
    const Encoding FORMAT = Encoding.Pcm16bit;
    const int BUFFER_SIZE = 16 * 1024;

    readonly QueuedStream _stream = new();
    readonly AudioRecord _record;
    //readonly int _bufferSize;

    public AndroidMicrophone()
    {
        //_bufferSize = AudioRecord.GetMinBufferSize(SAMPLING_RATE_IN_HZ, CHANNELS, FORMAT) * 2;
        _record = new AudioRecord(AudioSource.Mic, SAMPLING_RATE_IN_HZ, CHANNELS, FORMAT, BUFFER_SIZE);
    }

    public async void Start()
    {
        await CheckPermission();
        _record.StartRecording();

        _ = Task.Run(() =>
        {
            while (true)
            {
                var bytes = new byte[BUFFER_SIZE];
                int bytesRead = _record.Read(bytes, 0, BUFFER_SIZE);
                if (bytesRead < 0)
                {
                    throw new Exception($"Error reading audio, error code: {bytesRead}");
                }
                if (bytesRead > 0)
                {
                    _stream.Enqueue(bytes);
                }
            }
        });
    }

    public System.IO.Stream GetAudio() => _stream;
}
