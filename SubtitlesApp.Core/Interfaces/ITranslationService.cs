using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Interfaces;

public interface ITranslationService
{
    IAsyncEnumerable<Result<Subtitle>> TranslateAsync(
        List<Subtitle> sourceSubtitles,
        Language targetLanguage,
        CancellationToken cancellationToken = default
    );
}
