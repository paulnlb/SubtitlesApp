using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;
using SubtitlesServer.Application.Interfaces;
using Whisper.net.Ggml;
using Whisper.net;
using Microsoft.AspNetCore.SignalR;

namespace SubtitlesServer.Hubs;

public class WhisperHub : Hub
{
    public async Task TranscribeAudio([FromKeyedServices("wave")] IWaveService waveService, IAsyncEnumerable<byte[]> dataChunks, TrimmedAudioMetadataDTO audioMetadata)
    {
        Console.WriteLine("Connected");

        using var waveStream = await waveService.WriteToWaveStreamAsync(dataChunks, audioMetadata, CancellationToken.None);

        // whisper load
        var modelPath = Path.Combine(".", "WhisperModels", "ggml-small.bin");
        if (!File.Exists(modelPath))
        {
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Small);
            using var fileWriter = File.OpenWrite(modelPath);
            await modelStream.CopyToAsync(fileWriter);
        }

        using var whisperFactory = WhisperFactory.FromPath(modelPath);

        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("en")
            .Build();

        Console.WriteLine("Whisper loaded");

        var segments = processor.ProcessAsync(waveStream);

        await Clients.Caller.SendAsync("SetStatus", "Transcribing...");

        Console.WriteLine("Starting transcribing...");

        await foreach (var result in segments)
        {
            var subtitle = new Subtitle()
            {
                StartTime = result.Start + audioMetadata.StartTimeOffset,
                Text = result.Text,
                EndTime = result.End + audioMetadata.StartTimeOffset,
            };

            Console.WriteLine($"{subtitle.StartTime}: {subtitle.Text}");

            await Clients.Caller.SendAsync("ShowSubtitle", subtitle);
        }

        await Clients.Caller.SendAsync("SetStatus", "Done.");
    }
}
