using AVFoundation;
using Foundation;
using Stream = System.IO.Stream;

namespace MyPal.MauiApp;

class Sound
{
    public static async Task Play(Stream stream)
    {
        var taskCompletionSource = new TaskCompletionSource();
        var nsdata = NSData.FromStream(stream);
        if (nsdata is null)
        {
            throw new InvalidOperationException("Failed to create NSData from stream");
        }
        var player = new AVAudioPlayer(nsdata, "mp3", out var error);
        if (error is not null)
        {
            throw new InvalidOperationException($"Failed to create player: {error.LocalizedDescription}");
        }
        player.PrepareToPlay();
        player.Play();
        player.FinishedPlaying += (sender, e) =>
        {
            taskCompletionSource.TrySetResult();
        };
        await taskCompletionSource.Task;
    }
}
