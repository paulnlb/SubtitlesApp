using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Interfaces;

public interface ITranslationService
{
    IAsyncEnumerable<Result<SubtitleDto>> TranslateAsync(
        List<SubtitleDto> sourceSubtitles,
        string targetLanguageCode,
        CancellationToken cancellationToken = default
    );
}
