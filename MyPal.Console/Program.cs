using MyPal.ClassLibrary;
using NAudio.Wave;
using System.Collections.Concurrent;

var client = new MyPalWebClient();

await client.StartConversation(new Microphone(), new Speaker());

Console.ReadLine();

class Microphone : IMicrophone
{
    readonly WaveInEvent _waveInEvent;
    BufferStream _stream = new();

    public Microphone()
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

class BufferStream : Stream
{
    ConcurrentQueue<byte[]> _buffers = new();

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Flush() => throw new NotImplementedException();

    public void Enqueue(byte[] buffer) => _buffers.Enqueue(buffer);

    public override int Read(byte[] buffer, int offset, int count)
    {
        byte[]? bytes;
        while (!_buffers.TryDequeue(out bytes))
        {
            Thread.Sleep(100);
        }

        int length = Math.Min(bytes.Length, count);
        Array.Copy(bytes, 0, buffer, offset, length);
        return length;
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

    public override void SetLength(long value) => throw new NotImplementedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();
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