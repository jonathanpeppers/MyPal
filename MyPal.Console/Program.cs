using MyPal.ClassLibrary;
using NetCoreAudio;

var client = new MyPalWebClient();
var response = await client.SendImage("https://m.media-amazon.com/images/I/A1UZj5ww2YL._SY450_CR112%2C0%2C450%2C450_.jpg");
Console.WriteLine(response);

foreach (var voice in client.GetVoices())
{
    var stream = await client.TextToSpeech(response, voice);
    var path = @"D:\Downloads\MyPal.mp3";
    using (var fileStream = File.Create(path))
    {
        await stream.CopyToAsync(fileStream);
    }

    Console.WriteLine($"Playing {voice} voice...");
    var player = new Player();
    await player.Play(path);

    Console.ReadLine();
}
