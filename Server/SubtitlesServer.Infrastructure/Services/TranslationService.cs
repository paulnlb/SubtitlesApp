using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Infrastructure.Services;

public class TranslationService() : ITranslationService
{
    public Task<List<SubtitleDTO>> TranslateAsync(TranslationRequestDto requestDto)
    {
        // temporary mock implementation

        var subtitles = requestDto.SourceSubtitles;

        foreach (var subtitle in subtitles)
        {
            subtitle.Translation = new Translation
            {
                Text = $"[Translated to {requestDto.TargetLanguageCode}]" + subtitle.Text,
                LanguageCode = requestDto.TargetLanguageCode
            };
            subtitle.IsTranslated = false;
        }

        return Task.FromResult(subtitles);
    }
}
