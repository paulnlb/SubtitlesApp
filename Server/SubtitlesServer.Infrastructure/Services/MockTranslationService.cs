using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Infrastructure.Services;

public class MockTranslationService : ITranslationService
{
    public Task<ListResult<SubtitleDto>> TranslateAsync(TranslationRequestDto requestDto)
    {
        // temporary mock implementation

        var subtitles = requestDto.SourceSubtitles;
        var translatedSubtitlesDtos = new List<SubtitleDto>();

        foreach (var subtitle in subtitles)
        {
            translatedSubtitlesDtos.Add(
                new SubtitleDto
                {
                    Text = $"[Translated to {requestDto.TargetLanguageCode}]" + subtitle.Text,
                    LanguageCode = requestDto.TargetLanguageCode,
                    StartTime = subtitle.StartTime,
                    EndTime = subtitle.EndTime,
                }
            );
        }

        return Task.FromResult(ListResult<SubtitleDto>.Success(subtitles));
    }
}
