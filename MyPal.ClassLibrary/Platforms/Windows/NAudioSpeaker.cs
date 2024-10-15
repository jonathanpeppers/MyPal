using NAudio.Wave;

namespace MyPal.ClassLibrary;

public class NAudioSpeaker : ISpeaker, IDisposable
{
    readonly BufferedWaveProvider _waveProvider;
    readonly WaveOutEvent _waveOutEvent;

    public NAudioSpeaker()
    {
        var outputAudioFormat = new WaveFormat(
            rate: 24000,
            bits: 16,
            channels: 1);
        _waveProvider = new(outputAudioFormat)
        {
            BufferDuration = TimeSpan.FromMinutes(2),
        };
        _waveOutEvent = new();
        _waveOutEvent.Init(_waveProvider);
        _waveOutEvent.Play();
    }

    public void Play(BinaryData data)
    {
        byte[] buffer = data.ToArray();
        _waveProvider.AddSamples(buffer, 0, buffer.Length);
    }

    public void Stop()
    {
        _waveProvider.ClearBuffer();
    }

    public void Dispose()
    {
        _waveOutEvent.Dispose();
    }
}
