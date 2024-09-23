using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Configs;
using SubtitlesServer.Application.Interfaces;
using System.Runtime.CompilerServices;

namespace SubtitlesServer.Hubs;

public class WhisperHub : Hub
{
    public async IAsyncEnumerable<SubtitleDTO> TranscribeAudio(
        ITranscriptionService transcriptionService,
        IOptions<SpeechToTextConfigs> speechToTextConfig,
        IOptions<WhisperConfigs> whisperConfigs,
        IAsyncEnumerable<byte[]> dataChunks,
        TrimmedAudioMetadataDTO audioMetadata,
        [EnumeratorCancellation]
        CancellationToken cancellationToken)
    {
        Console.WriteLine("Connected");

        var subtitles = transcriptionService.TranscribeAudioAsync(
            dataChunks,
            audioMetadata,
            speechToTextConfig.Value,
            whisperConfigs.Value,
            cancellationToken);

        await Clients.Caller.SendAsync("SetStatus", "Transcribing...", cancellationToken: cancellationToken);

        await foreach (var subtitle in subtitles)
        {
            var subtitleDto = new SubtitleDTO
            {
                TimeInterval = new TimeIntervalDTO()
                {
                    StartTime = subtitle.TimeInterval.StartTime + audioMetadata.StartTimeOffset,
                    EndTime = subtitle.TimeInterval.EndTime + audioMetadata.StartTimeOffset
                },
                Text = subtitle.Text
            };

            Console.Write($"=> ");

            yield return subtitleDto;

            Console.Write($"{subtitleDto.TimeInterval.StartTime}: {subtitleDto.Text}\n");
        }

        await Clients.Caller.SendAsync("SetStatus", "Done.", cancellationToken: cancellationToken);

        Console.WriteLine("Transcribing done.");
    }
}
