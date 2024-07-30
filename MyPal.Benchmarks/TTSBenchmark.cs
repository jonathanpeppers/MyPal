using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using MyPal.ClassLibrary;

namespace MyPal.Benchmarks;

[MinIterationCount(1), MaxIterationCount(2)]
[SimpleJob(RunStrategy.ColdStart, launchCount: 2, warmupCount: 0, iterationCount: 2, invocationCount: 2)]
public class TTSBenchmark
{
    const string voice = "Fable";
    const string text = "Hi, how are you today?";
    readonly MyPalWebClient tts = new(hd: false);
    readonly MyPalWebClient tts_hd = new(hd: true);

    [Benchmark]
    public Task TSS() => tts.TextToSpeechAsync(text, voice);

    [Benchmark]
    public Task TTS_HD() => tts_hd.TextToSpeechAsync(text, voice);
}
