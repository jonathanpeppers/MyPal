﻿using MyPal.ClassLibrary;
using NAudio.Wave;
using System.Collections.Concurrent;

var client = new MyPalWebClient();

await client.StartConversation(new Microphone(), new Speaker());

Console.ReadLine();

class Microphone : IMicrophone
{
    readonly WaveInEvent _waveInEvent;
    QueuedStream _stream = new();

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