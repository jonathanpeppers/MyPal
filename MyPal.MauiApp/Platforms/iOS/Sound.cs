using AVFoundation;
using Foundation;
using Stream = System.IO.Stream;

namespace MyPal.MauiApp;

class Sound
{
    public static async Task Play(Stream stream)
    {
        var path = Path.Combine(Path.GetTempPath(), "media.mp3");
        using (var fileStream = File.Create(path))
            await stream.CopyToAsync(fileStream);
        var player = new AVAudioPlayer(NSUrl.FromFilename(path), "mp3", out var error);
        if (error is not null)
            throw new Exception(error.LocalizedDescription);
        player.PrepareToPlay();
        player.Play();
    }
}
