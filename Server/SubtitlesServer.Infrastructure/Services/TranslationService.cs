using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Services;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Infrastructure.Services;

public class TranslationService(LanguageService languageService) : ITranslationService
{
    public Task<List<SubtitleDTO>> TranslateAsync(TranslationRequestDto requestDto)
    {
        // temporary mock implementation

        var result = new List<SubtitleDTO>();

        foreach (var subtitle in requestDto.SourceSubtitles)
        {
            var translatedSub = new SubtitleDTO
            {
                TimeInterval = subtitle.TimeInterval,
                Text = $"[Translated to {requestDto.TargetLanguageCode}]" + subtitle.Text,
                Language = languageService.GetLanguageByCode(requestDto.TargetLanguageCode)
            };

            result.Add(translatedSub);
        }

        return Task.FromResult(result);
    }
}
