using Microsoft.AspNetCore.SignalR;
using SubtitlesApp.Shared.DTOs;
using SubtitlesServer.Application.Interfaces;
using static System.Net.Mime.MediaTypeNames;

namespace SubtitlesServer.Hubs;

public class WhisperHub : Hub
{
    public async Task TranscribeAudio(
        [FromKeyedServices("transcription")] ITranscriptionService transcriptionService,
        [FromKeyedServices("cancellationManager")] ICancellationManager cancellationManager,
        IAsyncEnumerable<byte[]> dataChunks,
        TrimmedAudioMetadataDTO audioMetadata)
    {
        Console.WriteLine("Connected");

        await Clients.Caller.SendAsync("SetStatus", "Transcribing...");

        var cancellationToken = cancellationManager.RegisterTask(Context.ConnectionId);

        var subtitles = transcriptionService.TranscribeAudioAsync(dataChunks, audioMetadata, cancellationToken);

        await foreach (var subtitle in subtitles)
        {
            Console.WriteLine($"{subtitle.TimeInterval.StartTime}: {subtitle.Text}");

            var subtitleDto = new SubtitleDTO
            {
                TimeInterval = new TimeIntervalDTO() { 
                    StartTime = subtitle.TimeInterval.StartTime,
                    EndTime = subtitle.TimeInterval.EndTime
                },
                Text = subtitle.Text
            };

            await Clients.Caller.SendAsync("ShowSubtitle", subtitleDto);
        }

        cancellationManager.RemoveTask(Context.ConnectionId);

        await Clients.Caller.SendAsync("SetStatusAndEditTimeline", "Done.", audioMetadata);

        Console.WriteLine("Transcribing done.");
    }

    public async Task CancelTranscription([FromKeyedServices("cancellationManager")] ICancellationManager cancellationManager)
    {
        cancellationManager.CancelTask(Context.ConnectionId);

        await Clients.Caller.SendAsync("SetStatus", "Transcritption cancelled.");
    }
}
