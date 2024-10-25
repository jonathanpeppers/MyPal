using MyPal.ClassLibrary;
using Plugin.Maui.Audio;

namespace MyPal.MauiApp;

/// <summary>
/// Implements ISpeaker with Plugin.Maui.Audio
/// </summary>
class MauiSpeaker : ISpeaker
{
    readonly IAudioManager _audio;
    IAudioPlayer? _player;

    public MauiSpeaker(IAudioManager audio)
    {
        _audio = audio;
    }

    public void Play(BinaryData data)
    {
        Stop();

        var player = 
            _player = _audio.CreatePlayer(data.ToStream());
        player.Play();
    }

    public void Stop()
    {
        if (_player is not null)
        {
            _player.Stop();
            _player = null;
        }
    }
}
