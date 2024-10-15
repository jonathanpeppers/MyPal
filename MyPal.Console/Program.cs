using MyPal.ClassLibrary;

var client = new MyPalWebClient();

await client.StartConversation(new NAudioMicrophone(), new NAudioSpeaker());

Console.ReadLine();
