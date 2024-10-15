using NAudio.Wave;

namespace MyPal.ClassLibrary;

/// <summary>
/// Note this is useable in net8.0 or net8.0-windows
/// </summary>
public class NAudioMicrophone : IMicrophone
{
    readonly WaveInEvent _waveInEvent;
    QueuedStream _stream = new();

    public NAudioMicrophone()
    {
        _waveInEvent = new()
        {
            WaveFormat = new WaveFormat(rate: 24000, bits: 2 * 8, channels: 1)
        };
        _waveInEvent.DataAvailable += (_, e) =>
        {
            _stream.Enqueue(e.Buffer);
        };
        _waveInEvent.StartRecording();
    }

    public Stream GetAudio() => _stream;
}
