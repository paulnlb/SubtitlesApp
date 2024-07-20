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

        var processor = whisperFactory.CreateBuilder()
            .WithLanguage("en")
            .Build();

        Console.WriteLine("Whisper loaded");

        var segments = processor.ProcessAsync(audioStream, cancellationToken);

        Console.WriteLine("Starting transcribing...");

        try
        {
            await foreach (var result in segments)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var startTime = result.Start + startTimeOffset;
                var endTime = result.End + startTimeOffset;

                var subtitle = new Subtitle()
                {
                    Text = result.Text,
                    TimeInterval = new TimeInterval(startTime, endTime),
                };

                yield return subtitle;
            }
        }
        finally
        {
            // Dispose the processor explicitly using Async pattern 
            // to support cancellation.
            await processor.DisposeAsync();
        }
    }
}
