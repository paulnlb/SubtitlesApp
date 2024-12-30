using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface ITranslationService
{
    Task<Result<List<SubtitleDTO>>> TranslateAsync(
        List<SubtitleDTO> subtitlesToTranslate,
        string targetLanguageCode,
        CancellationToken cancellationToken = default);
}
