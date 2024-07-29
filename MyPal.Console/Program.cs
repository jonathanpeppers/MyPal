using MyPal.ClassLibrary;

var client = new MyPalWebClient();
var response = await client.SendImage("https://m.media-amazon.com/images/I/A1UZj5ww2YL._SY450_CR112%2C0%2C450%2C450_.jpg");
Console.WriteLine(response);
