using Microsoft.AspNetCore.SignalR;
using SubtitlesApp.Shared.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Hubs;

public class WhisperHub : Hub
{
    public async Task TranscribeAudio(
        [FromKeyedServices("transcription")] ITranscriptionService transcriptionService,
        IAsyncEnumerable<byte[]> dataChunks,
        TrimmedAudioMetadataDTO audioMetadata)
    {
        Console.WriteLine("Connected");

        await Clients.Caller.SendAsync("SetStatus", "Transcribing...");

        var subtitles = transcriptionService.TranscribeAudioAsync(dataChunks, audioMetadata);

        await foreach (var subtitle in subtitles)
        {
            Console.WriteLine($"{subtitle.StartTime}: {subtitle.Text}");
            await Clients.Caller.SendAsync("ShowSubtitle", subtitle);
        }
        await Clients.Caller.SendAsync("SetStatus", "Done.");
    }
}
