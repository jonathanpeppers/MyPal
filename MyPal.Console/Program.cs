﻿using MyPal.ClassLibrary;
using NetCoreAudio;

var client = new MyPalWebClient();
var response = await client.SendVideoAsync(@"D:\src\MyPal\assets\test.mov");
Console.WriteLine(response);

var stream = await client.TextToSpeechAsync(response, "Fable");
var path = @"D:\Downloads\MyPal.mp3";
using (var fileStream = File.Create(path))
{
    await stream.CopyToAsync(fileStream);
}

var player = new Player();
await player.Play(path);

Console.ReadLine();