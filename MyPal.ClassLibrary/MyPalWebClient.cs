using Azure.AI.OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using OpenAI.RealtimeConversation;
using System.ClientModel;
using System.Text.RegularExpressions;

namespace MyPal.ClassLibrary;

public partial class MyPalWebClient
{
    readonly AzureOpenAIClient _client;
    readonly ChatClient _chat;
    readonly AudioClient _audio;
    readonly RealtimeConversationClient _realtime;

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
        _realtime = _client.GetRealtimeConversationClient("gpt-4o-realtime-preview");
    }

    public Task<string> SendImageAsync(string filePath, bool insult)
    {
        using var stream = File.OpenRead(filePath);
        return SendImageAsync(stream, insult);
    }

    const string Prompt = "You are a funny character that enjoys {0}ing your friends in a very fun way. Do not comment about the photo being blurry, as it is purposely low-resolution. Don't use ellipsis such as '...' or emojis, and punctuate sentences with only a single '?', '.' or '!'. All {0}s are kid-friendly, {0} the photo appropriately using details as much as possible:";

    public async Task<string> SendImageAsync(Stream stream, bool insult)
    {
        var data = await BinaryData.FromStreamAsync(stream);
        var result = await _chat.CompleteChatAsync([
            new SystemChatMessage(string.Format(Prompt, insult ? "insult" : "compliment")),
            new UserChatMessage(ChatMessageContentPart.CreateImagePart(data, "image/jpg", ChatImageDetailLevel.Auto)),
        ]);
        return result.Value.Content[0].ToString() ?? "";
    }

    public string[] GetVoices()
    {
        return [
            nameof(GeneratedSpeechVoice.Alloy),
            nameof(GeneratedSpeechVoice.Echo),
            nameof(GeneratedSpeechVoice.Fable),
            nameof(GeneratedSpeechVoice.Nova),
            nameof(GeneratedSpeechVoice.Onyx),
            nameof(GeneratedSpeechVoice.Shimmer),
        ];
    }

    public async Task<Stream> TextToSpeechAsync(string text, string voice)
    {
        text = text.Trim();
        var result = await _audio.GenerateSpeechAsync(text, ToVoice(voice), new SpeechGenerationOptions { ResponseFormat = GeneratedSpeechFormat.Mp3 });
        return result.Value.ToStream();
    }

    static GeneratedSpeechVoice ToVoice(string voice)
    {
        GeneratedSpeechVoice speechVoice;
        switch (voice)
        {
            case "Alloy":
                speechVoice = GeneratedSpeechVoice.Alloy;
                break;
            case "Echo":
                speechVoice = GeneratedSpeechVoice.Echo;
                break;
            case "Fable":
                speechVoice = GeneratedSpeechVoice.Fable;
                break;
            case "Nova":
                speechVoice = GeneratedSpeechVoice.Nova;
                break;
            case "Onyx":
                speechVoice = GeneratedSpeechVoice.Onyx;
                break;
            case "Shimmer":
                speechVoice = GeneratedSpeechVoice.Shimmer;
                break;
            default:
                throw new InvalidOperationException($"Unable to find voice named: {voice}!");
        }

        return speechVoice;
    }

    public async IAsyncEnumerable<Stream> SendImageStreaming(string filePath, string voice, bool insult)
    {
        using var stream = File.OpenRead(filePath);
        await foreach (var item in SendImageStreaming(stream, voice, insult))
        {
            yield return item;
        }
    }

    [GeneratedRegex(@"[\.|\?|\!]", RegexOptions.CultureInvariant)]
    private static partial Regex PunctuationRegex();

    public async IAsyncEnumerable<Stream> SendImageStreaming(Stream stream, string voice, bool insult)
    {
        var data = await BinaryData.FromStreamAsync(stream);
        string text = "";
        await foreach (StreamingChatCompletionUpdate result in _chat.CompleteChatStreamingAsync([
            new SystemChatMessage(string.Format(Prompt, insult ? "insult" : "compliment")),
            new UserChatMessage(ChatMessageContentPart.CreateImagePart(data, "image/jpg", ChatImageDetailLevel.Auto)),
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
                    string tts = text[..(match.Index + match.Length)].Trim();
                    Console.WriteLine("TTS: " + text);
                    yield return await TextToSpeechAsync(tts, voice);
                    text = text[(match.Index + match.Length)..];
                }
            }
        }

        // If any leftover text, convert to speech
        text = text.Trim();
        if (!string.IsNullOrEmpty(text))
            yield return await TextToSpeechAsync(text, voice);
    }

    /// <summary>
    /// From: https://github.com/Azure-Samples/aoai-realtime-audio-sdk/blob/6b0382442f43b25f3ccb4f034a3d78d37b701a62/dotnet/samples/console-from-mic/Program.cs
    /// </summary>
    public async Task StartConversation()
    {
        var session = await _realtime.StartConversationSessionAsync();

        await session.ConfigureSessionAsync(new ConversationSessionOptions
        {
            InputTranscriptionOptions = new ConversationInputTranscriptionOptions
            {
                Model = "whisper-1"
            }
        });

        // With the session configured, we start processing commands received from the service.
        await foreach (ConversationUpdate update in session.ReceiveUpdatesAsync())
        {
            if (update is ConversationSessionStartedUpdate sessionStarted)
            {

            }
            else if (update is ConversationInputSpeechStartedUpdate speechStarted)
            {

            }
            else if (update is ConversationInputSpeechFinishedUpdate speechFinished)
            {

            }
            else if (update is ConversationInputTranscriptionFinishedUpdate transcriptionFinished)
            {

            }
            else if (update is ConversationAudioDeltaUpdate deltaUpdate)
            {

            }
            else if (update is ConversationOutputTranscriptionDeltaUpdate transcriptionDeltaUpdate)
            {

            }
            else if (update is ConversationItemFinishedUpdate itemFinished)
            {

            }
            else if (update is ConversationErrorUpdate error)
            {

            }
        }
    }
}
