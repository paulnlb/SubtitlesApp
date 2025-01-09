using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface ITranslationService
{
    Task<ListResult<SubtitleDto>> TranslateAsync(
        List<SubtitleDto> subtitlesToTranslate,
        string targetLanguageCode,
        CancellationToken cancellationToken = default
    );

    Task<AsyncEnumerableResult<SubtitleDto>> TranslateAndStreamAsync(
        List<SubtitleDto> subtitlesToTranslate,
        string targetLanguageCode,
        CancellationToken cancellationToken = default
    );
}
