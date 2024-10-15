using MyPal.ClassLibrary;
using NAudio.Wave;

var client = new MyPalWebClient();

await client.StartConversation(new Microphone(), new Speaker());

Console.ReadLine();

class Microphone : IMicrophone
{
    const int SAMPLES_PER_SECOND = 24000;
    const int BYTES_PER_SAMPLE = 2;
    const int CHANNELS = 1;

    readonly WaveInEvent _waveInEvent;
    readonly byte[] _buffer = new byte[BYTES_PER_SAMPLE * SAMPLES_PER_SECOND * CHANNELS * 10];
    readonly object _bufferLock = new();
    int _position = 0;

    public Microphone()
    {
        _waveInEvent = new()
        {
            WaveFormat = new WaveFormat(SAMPLES_PER_SECOND, BYTES_PER_SAMPLE * 8, CHANNELS),
        };
        _waveInEvent.DataAvailable += (_, e) =>
        {
            lock (_bufferLock)
            {
                int bytesToCopy = e.BytesRecorded;
                if (_position + bytesToCopy >= _buffer.Length)
                {
                    int bytesToCopyBeforeWrap = _buffer.Length - _position;
                    Array.Copy(e.Buffer, 0, _buffer, _position, bytesToCopyBeforeWrap);
                    bytesToCopy -= bytesToCopyBeforeWrap;
                    _position = 0;
                }
                Array.Copy(e.Buffer, e.BytesRecorded - bytesToCopy, _buffer, _position, bytesToCopy);
                _position += bytesToCopy;
            }
        };
        _waveInEvent.StartRecording();
    }

    public Stream GetAudio()
    {
        // Wait until the buffer is at least half full
        while (_position < _buffer.Length / 2)
        {
            Thread.Sleep(100);
        }

        lock (_bufferLock)
        {
            return new MemoryStream(_buffer, 0, _position);
        }
    }
}

class Speaker : ISpeaker, IDisposable
{
    readonly BufferedWaveProvider _waveProvider;
    readonly WaveOutEvent _waveOutEvent;

    public Speaker()
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