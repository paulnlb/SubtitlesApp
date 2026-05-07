using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Interfaces;

public interface ITranslationService
{
    IAsyncEnumerable<Result<SubtitleDto>> TranslateAsync(
        List<SubtitleDto> sourceSubtitles,
        Language targetLanguage,
        CancellationToken cancellationToken = default
    );
}
