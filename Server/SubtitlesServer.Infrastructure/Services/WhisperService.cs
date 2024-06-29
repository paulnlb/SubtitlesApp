using SubtitlesApp.Core.Models;
using SubtitlesServer.Application.Interfaces;
using System.Runtime.CompilerServices;
using Whisper.net;
using Whisper.net.Ggml;

namespace SubtitlesServer.Infrastructure.Services;

public class WhisperService : IWhisperService
{
    public async IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        MemoryStream audioStream, 
        TimeSpan startTimeOffset,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // whisper load
        var modelPath = Path.Combine("..", "SubtitlesServer.Infrastructure", "WhisperModels", "ggml-small.bin");
        if (!File.Exists(modelPath))
        {
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(
                GgmlType.Small,
                QuantizationType.NoQuantization,
                cancellationToken);

            using var fileWriter = File.OpenWrite(modelPath);
            await modelStream.CopyToAsync(fileWriter, cancellationToken);
        }

        using var whisperFactory = WhisperFactory.FromPath(modelPath);

        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("en")
            .Build();

        Console.WriteLine("Whisper loaded");

        var segments = processor.ProcessAsync(audioStream, cancellationToken);

        Console.WriteLine("Starting transcribing...");

        await foreach (var result in segments)
        {
            var subtitle = new Subtitle()
            {
                StartTime = result.Start + startTimeOffset,
                Text = result.Text,
                EndTime = result.End + startTimeOffset,
            };

            yield return subtitle;
        }
    }
}
