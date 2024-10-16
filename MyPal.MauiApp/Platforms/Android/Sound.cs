﻿using Android.Media;
using Stream = System.IO.Stream;

namespace MyPal.MauiApp;

class Sound
{
    public static async Task Play(Stream stream)
    {
        var taskCompletionSource = new TaskCompletionSource();
        var path = Path.Combine(Path.GetTempPath(), "media.mp3");
        using (var fileStream = File.Create(path))
            await stream.CopyToAsync(fileStream);
        var player = new MediaPlayer();
        player.SetDataSource(path);
        player.Prepare();
        player.Start();
        player.Completion += (sender, e) =>
        {
            taskCompletionSource.TrySetResult();
        };
        await taskCompletionSource.Task;
    }
}
