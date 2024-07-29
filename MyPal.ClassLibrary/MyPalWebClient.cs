using Azure.AI.OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using System.ClientModel;

namespace MyPal.ClassLibrary;

public class MyPalWebClient
{
    readonly AzureOpenAIClient _client;
    readonly ChatClient _chat;
    readonly AudioClient _audio;

    public MyPalWebClient(string apiKey = "")
    {
        if (string.IsNullOrEmpty (apiKey))
        {
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
        }
        if (string.IsNullOrEmpty(apiKey))
        {
            ArgumentNullException.ThrowIfNull(apiKey, nameof(apiKey));
        }
        _client = new AzureOpenAIClient(new Uri("https://icropenaiservice2.openai.azure.com/"), new ApiKeyCredential(apiKey));
        _chat = _client.GetChatClient("icrgpt-4o");
        _audio = _client.GetAudioClient("icrtts-hd");
    }

    public async Task<string> SendImage(string url)
    {
        var result = await _chat.CompleteChatAsync([
            new SystemChatMessage("You are a funny character that enjoys insulting your friends in a very fun way. All insults are kid-friendly, insult the photo appropriately:"),
            new UserChatMessage(ChatMessageContentPart.CreateImageMessageContentPart(new Uri(url))),
        ]);
        return result.Value.Content[0].ToString();
    }

    public async Task<Stream> TextToSpeech(string text)
    {
        var result = await _audio.GenerateSpeechFromTextAsync(text, GeneratedSpeechVoice.Nova, new SpeechGenerationOptions { ResponseFormat = GeneratedSpeechFormat.Mp3 });
        return result.Value.ToStream();
    }
}
