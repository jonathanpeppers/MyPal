using Azure.AI.OpenAI;
using OpenAI.RealtimeConversation;
using System.ClientModel;
using System.Diagnostics;

namespace MyPal.ClassLibrary;

public partial class MyPalWebClient
{
    readonly AzureOpenAIClient _client;
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
        _client = new AzureOpenAIClient(new Uri("https://mypalopenai.openai.azure.com/openai/realtime?api-version=2024-10-01-preview&deployment=gpt-4o-realtime-preview"), new ApiKeyCredential(apiKey));
        _realtime = _client.GetRealtimeConversationClient("gpt-4o-realtime-preview");
    }

    /// <summary>
    /// From: https://github.com/Azure-Samples/aoai-realtime-audio-sdk/blob/6b0382442f43b25f3ccb4f034a3d78d37b701a62/dotnet/samples/console-from-mic/Program.cs
    /// </summary>
    public async Task StartConversation(IMicrophone microphone, ISpeaker speaker, ICharacter? character = null, bool insult = true)
    {
        microphone.Start();

        var session = await _realtime.StartConversationSessionAsync();

        await session.ConfigureSessionAsync(new ConversationSessionOptions
        {
            Instructions = string.Format("You are a funny character that enjoys {0}ing your friends in a very fun way. All {0}s are kid-friendly, {0} me based on my comments.", insult ? "insult" : "compliment"),
            Temperature = 1,
            InputTranscriptionOptions = new ConversationInputTranscriptionOptions
            {
                Model = "whisper-1"
            },
            Voice = ConversationVoice.Alloy,
        });

        // With the session configured, we start processing commands received from the service.
        await foreach (ConversationUpdate update in session.ReceiveUpdatesAsync())
        {
            if (update is ConversationSessionStartedUpdate sessionStarted)
            {
                Debug.WriteLine($"Starting session with {sessionStarted.Voice}...");

                _ = Task.Run(() =>
                {
                    var audio = microphone.GetAudio();
                    session.SendAudioAsync(audio);
                });
            }
            else if (update is ConversationInputSpeechStartedUpdate speechStarted)
            {
                Debug.WriteLine("Start of speech detected...");
                speaker.Stop();
            }
            else if (update is ConversationInputSpeechFinishedUpdate speechFinished)
            {
                Debug.WriteLine("End of speech detected...");
            }
            else if (update is ConversationInputTranscriptionFinishedUpdate transcriptionFinished)
            {
                Debug.WriteLine($"Transcription: {transcriptionFinished.Transcript}");
            }
            else if (update is ConversationAudioDeltaUpdate deltaUpdate)
            {
                Debug.WriteLine("Audio delta received, playing...");
                speaker.Play(deltaUpdate.Delta);
                character?.StartTalking();
            }
            else if (update is ConversationOutputTranscriptionDeltaUpdate transcriptionDeltaUpdate)
            {
                Debug.WriteLine($"Delta transcription: {transcriptionDeltaUpdate.Delta}");
            }
            else if (update is ConversationItemFinishedUpdate itemFinished)
            {
                if (!string.IsNullOrEmpty(itemFinished.FunctionName))
                {
                    Debug.WriteLine($"Finished running custom function: {itemFinished.FunctionName}");
                }
                else
                {
                    Debug.WriteLine("Continuing conversation...");
                }
                
                // TODO: this isn't right
                //character?.Idle();
            }
            else if (update is ConversationErrorUpdate error)
            {
                Debug.WriteLine($"Error: {error.GetRawContent()}");
                throw new Exception($"{error.ErrorCode}: {error.ErrorMessage}");
            }
        }
    }
}
