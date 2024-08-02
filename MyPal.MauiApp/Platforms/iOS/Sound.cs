using AVFoundation;
using Foundation;
using Stream = System.IO.Stream;

namespace MyPal.MauiApp;

class Sound
{
    public static async Task Play(Stream stream)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();
        var nsdata = NSData.FromStream(stream);
        if (nsdata is null)
        {
            Console.WriteLine("Failed to create NSData from stream");
            return;
        }
        var player = new AVAudioPlayer(nsdata, "mp3", out var error);
        if (error is not null)
        {
            Console.WriteLine($"Failed to create player: {error.LocalizedDescription}");
            return;
        }
        player.PrepareToPlay();
        player.Play();
        player.FinishedPlaying += (sender, e) =>
        {
            taskCompletionSource.SetResult(true);
        };
        await taskCompletionSource.Task;
    }
}
