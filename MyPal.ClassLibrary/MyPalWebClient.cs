using Azure.AI.OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.RegularExpressions;

namespace MyPal.ClassLibrary;

public partial class MyPalWebClient
{
    readonly AzureOpenAIClient _client;
    readonly ChatClient _chat;
    readonly AudioClient _audio;

    public MyPalWebClient(string? apiKey = "", bool hd = false)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = AppContext.GetData("OPENAI_API_KEY") as string;
        }
        if (string.IsNullOrEmpty (apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
        }
        if (string.IsNullOrEmpty(apiKey))
        {
            ArgumentNullException.ThrowIfNull(apiKey, nameof(apiKey));
        }
        _client = new AzureOpenAIClient(new Uri("https://icropenaiservice2.openai.azure.com/"), new ApiKeyCredential(apiKey));
        _chat = _client.GetChatClient("gpt-4o");
        _audio = _client.GetAudioClient(hd ? "tts-hd" : "tts");
    }

    public Task<string> SendImageAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        return SendImageAsync(stream);
    }

    const string Prompt = "You are a funny character that enjoys insulting your friends in a very fun way. Do not comment about the photo being blurry, as it is purposely low-resolution. Don't use ellipsis such as '...' or emojis, and punctuate sentences with only a single '?', '.' or '!'. All insults are kid-friendly, insult the photo appropriately using details as much as possible:";

    public async Task<string> SendImageAsync(Stream stream)
    {
        var data = await BinaryData.FromStreamAsync(stream);
        var result = await _chat.CompleteChatAsync([
            new SystemChatMessage(Prompt),
            new UserChatMessage(ChatMessageContentPart.CreateImageMessageContentPart(data, "image/jpg", ImageChatMessageContentPartDetail.Auto)),
        ]);
        return result.Value.Content[0].ToString();
    }

    public string[] GetVoices()
    {
        return Enum.GetNames<GeneratedSpeechVoice>();
    }

    public async Task<Stream> TextToSpeechAsync(string text, string voice)
    {
        text = text.Trim();
        var result = await _audio.GenerateSpeechFromTextAsync(text, Enum.Parse<GeneratedSpeechVoice>(voice), new SpeechGenerationOptions { ResponseFormat = GeneratedSpeechFormat.Mp3 });
        return result.Value.ToStream();
    }

    public async IAsyncEnumerable<Stream> SendImageStreaming(string filePath, string voice)
    {
        using var stream = File.OpenRead(filePath);
        await foreach (var item in SendImageStreaming(stream, voice))
        {
            yield return item;
        }
    }

    [GeneratedRegex(@"[\.|\?|\!]", RegexOptions.CultureInvariant)]
    private static partial Regex PunctuationRegex();

    public async IAsyncEnumerable<Stream> SendImageStreaming(Stream stream, string voice)
    {
        var data = await BinaryData.FromStreamAsync(stream);
        string text = "";
        await foreach (StreamingChatCompletionUpdate result in _chat.CompleteChatStreamingAsync([
            new SystemChatMessage(Prompt),
            new UserChatMessage(ChatMessageContentPart.CreateImageMessageContentPart(data, "image/jpg", ImageChatMessageContentPartDetail.Auto)),
        ]))
        {
            foreach (var item in result.ContentUpdate)
            {
                if (string.IsNullOrEmpty(item.Text))
                    continue;

                text += item.Text;

                var match = PunctuationRegex().Match(text);
                if (match.Success)
                {
                    string tts = text.Substring(0, match.Index + match.Length).Trim();
                    Console.WriteLine("TTS: " + text);
                    yield return await TextToSpeechAsync(tts, voice);
                    text = text.Substring(match.Index + match.Length);
                }
            }
        }

        // If any leftover text, convert to speech
        text = text.Trim();
        if (!string.IsNullOrEmpty(text))
            yield return await TextToSpeechAsync(text, voice);
    }
}
