using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Application.Services;

public class TranscriptionService(IWhisperService whisperService) : ITranscriptionService
{
    public async Task<List<SubtitleDTO>> TranscribeAudioAsync(
        byte[] audioBytes,
        string subtitlesLanguageCode,
        CancellationToken cancellationToken = default)
    {
        var subtitles =  whisperService.TranscribeAudioAsync(
            audioBytes,
            subtitlesLanguageCode,
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
                Text = subtitle.Text,
                LanguageCode = subtitle.LanguageCode
            };

            subtitlesList.Add(subtitleDto);
        }

        return subtitlesList;
    }
}
