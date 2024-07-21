using Microsoft.AspNetCore.SignalR;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Hubs;

public class WhisperMockHub : Hub
{
    public async Task TranscribeAudio(
        IAsyncEnumerable<byte[]> dataChunks,
        TrimmedAudioMetadataDTO audioMetadata)
    {
        Console.WriteLine("Connected");

        await Clients.Caller.SendAsync("SetStatus", "Transcribing...");

        await Task.Delay(1000);

        var max = audioMetadata.EndTime - audioMetadata.StartTimeOffset;

        Console.WriteLine($"Max: {max}");

        for (int i = 0; i < max.TotalSeconds; i+=2)
        {
            TimeSpan startTime;
            TimeSpan endTime;

            if (i % 10 == 0)
            {
                startTime = TimeSpan.FromSeconds(audioMetadata.StartTimeOffset.TotalSeconds + i + 1);
                endTime = TimeSpan.FromSeconds(audioMetadata.StartTimeOffset.TotalSeconds + i + 3);
            }
            else
            {
                startTime = TimeSpan.FromSeconds(audioMetadata.StartTimeOffset.TotalSeconds + i);
                endTime = TimeSpan.FromSeconds(audioMetadata.StartTimeOffset.TotalSeconds + i + 2);
            }

            var text = $"Subtitle ({startTime.Minutes}m, {startTime.Seconds}s)";

            var subtitle = new SubtitleDTO
            {
                TimeInterval = new TimeIntervalDTO() { StartTime = startTime, EndTime = endTime },
                Text = text
            };

            Console.WriteLine($"{subtitle.TimeInterval.StartTime}: {subtitle.Text}");

            await Clients.Caller.SendAsync("ShowSubtitle", subtitle);
        }

        await Clients.Caller.SendAsync("SetStatusAndEditTimeline", "Done.", audioMetadata);
    }

    public async Task CancelTranscription([FromKeyedServices("cancellationManager")] ICancellationManager cancellationManager)
    {
        await Clients.Caller.SendAsync("SetStatus", "Transcritption cancelled.");
    }
}
