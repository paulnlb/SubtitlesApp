using Microsoft.AspNetCore.SignalR;
using SubtitlesApp.Shared.DTOs;
using SubtitlesServer.Application.Interfaces;

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
            Console.WriteLine($"{subtitle.StartTime}: {subtitle.Text}");

            await Clients.Caller.SendAsync("ShowSubtitle", subtitle);
        }

        await Clients.Caller.SendAsync("SetStatus", "Done.");
    }

    public async Task CancelTranscription([FromKeyedServices("cancellationManager")] ICancellationManager cancellationManager)
    {
        cancellationManager.CancelTask(Context.ConnectionId);

        await Clients.Caller.SendAsync("SetStatus", "Transcritption cancelled.");
    }
}
