using Microsoft.AspNetCore.SignalR;
using SubtitlesApp.Shared.DTOs;
using SubtitlesServer.Application.Interfaces;
using System.Runtime.CompilerServices;

namespace SubtitlesServer.Hubs;

public class WhisperHub : Hub
{
    public async IAsyncEnumerable<SubtitleDTO> TranscribeAudio(
        [FromKeyedServices("transcription")] ITranscriptionService transcriptionService,
        IAsyncEnumerable<byte[]> dataChunks,
        TrimmedAudioMetadataDTO audioMetadata,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Connected");

        var subtitles = transcriptionService.TranscribeAudioAsync(dataChunks, audioMetadata, cancellationToken);

        await Clients.Caller.SendAsync("SetStatus", "Transcribing...", cancellationToken: cancellationToken);

        await foreach (var subtitle in subtitles)
        {
            var subtitleDto = new SubtitleDTO
            {
                TimeInterval = new TimeIntervalDTO()
                {
                    StartTime = subtitle.TimeInterval.StartTime,
                    EndTime = subtitle.TimeInterval.EndTime
                },
                Text = subtitle.Text
            };

            Console.Write($"=> ");

            yield return subtitleDto;

            Console.Write($"{subtitle.TimeInterval.StartTime}: {subtitle.Text}\n");
        }

        await Clients.Caller.SendAsync("SetStatus", "Done.", cancellationToken: cancellationToken);

        Console.WriteLine("Transcribing done.");
    }
}
