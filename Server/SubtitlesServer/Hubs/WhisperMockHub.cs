using Microsoft.AspNetCore.SignalR;
using SubtitlesApp.Shared.DTOs;
using System.Runtime.CompilerServices;

namespace SubtitlesServer.Hubs;

public class WhisperMockHub : Hub
{
    public async IAsyncEnumerable<SubtitleDTO> TranscribeAudio(
        IAsyncEnumerable<byte[]> dataChunks,
        TrimmedAudioMetadataDTO audioMetadata,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Connected");

        await Clients.Caller.SendAsync("SetStatus", "Transcribing...");

        var listChunks = new List<byte[]>();

        await foreach (var chunk in dataChunks)
        {
            listChunks.Add(chunk);
        }

        //await Task.Delay(1000);

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

            var text = $"Subtitle ({startTime.Minutes}m, {startTime.Seconds}s) \nTest";

            var subtitle = new SubtitleDTO
            {
                TimeInterval = new TimeIntervalDTO() { StartTime = startTime, EndTime = endTime },
                Text = text
            };

            Console.WriteLine($"{subtitle.TimeInterval.StartTime}: {subtitle.Text}");

            await Task.Delay(0, cancellationToken);

            yield return subtitle;
        }

        await Clients.Caller.SendAsync("SetStatus", "Done.");

        Console.WriteLine("Done");
    }
}
