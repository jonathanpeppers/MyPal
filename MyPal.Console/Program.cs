using MyPal.ClassLibrary;
using NetCoreAudio;
using System.Diagnostics;

var client = new MyPalWebClient();
var sw = new Stopwatch();
sw.Start();
int index = 0;
TaskCompletionSource? source = null;
await foreach (var stream in client.SendImageStreaming(@"D:\src\MyPal\assets\test.jpg", "Fable"))
{
    Console.WriteLine($"Time: {sw.ElapsedMilliseconds}ms");
    if (source != null)
    {
        await source.Task;
    }
    var path = @$"D:\Downloads\MyPal{index++}.mp3";
    using (var fileStream = File.Create(path))
    {
           await stream.CopyToAsync(fileStream);
    }
    var player = new Player();
    source = new TaskCompletionSource();
    player.PlaybackFinished += (sender, e) => source.TrySetResult();
    await player.Play(path);
}

Console.ReadLine();