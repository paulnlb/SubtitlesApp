using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Application.Services;

public class TranscriptionService : ITranscriptionService
{
    private readonly IWaveService _waveService;
    private readonly IWhisperService _whisperService;

    public TranscriptionService(IWaveService waveService, IWhisperService whisperService)
    {
        _waveService = waveService;
        _whisperService = whisperService;
    }

    public async Task<List<SubtitleDTO>> TranscribeAudioAsync(
        TrimmedAudioDto audioMetadata,
        CancellationToken cancellationToken = default)
    {
        using var waveStream = await _waveService.WriteToWaveStreamAsync(audioMetadata, cancellationToken);

        var subtitles = _whisperService.TranscribeAudioAsync(
            waveStream,
            cancellationToken);

        var subtitlesList = new List<SubtitleDTO>();

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

            subtitlesList.Add(subtitleDto);
        }

        return subtitlesList;
    }
}
