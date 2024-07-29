using MyPal.ClassLibrary;
using System.Media;

var client = new MyPalWebClient();
var response = await client.SendImage("https://m.media-amazon.com/images/I/A1UZj5ww2YL._SY450_CR112%2C0%2C450%2C450_.jpg");
Console.WriteLine(response);

var stream = await client.TextToSpeech(response);
var path = @"D:\Downloads\MyPal.wav";
using (var fileStream = File.Create(path))
{
    await stream.CopyToAsync(fileStream);
}

if (OperatingSystem.IsWindows())
{
    stream.Seek(0, SeekOrigin.Begin);

    var player = new SoundPlayer(path);
    player.LoadCompleted += (sender, e) =>
    {
        if (e.Error != null)
        {
            Console.WriteLine(e.Error.Message);
        }
        else
        {
            Console.WriteLine("Loaded audio!");
        }
    };
    player.PlaySync();
}
else
{
    throw new PlatformNotSupportedException();
}

Console.ReadLine();
