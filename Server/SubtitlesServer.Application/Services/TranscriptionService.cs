using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Application.Services;

public class TranscriptionService(IWhisperService whisperService) : ITranscriptionService
{
    public async Task<List<SubtitleDTO>> TranscribeAudioAsync(
        byte[] audioBytes,
        CancellationToken cancellationToken = default)
    {
        var subtitles =  whisperService.TranscribeAudioAsync(
            audioBytes,
            cancellationToken);

        var subtitlesList = new List<SubtitleDTO>();

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

            subtitlesList.Add(subtitleDto);
        }

        return subtitlesList;
    }
}
