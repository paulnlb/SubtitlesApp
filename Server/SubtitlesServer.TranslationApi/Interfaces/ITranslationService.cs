using SubtitlesServer.Shared.Models;
using SubtitlesServer.Shared.Result;

namespace SubtitlesServer.TranslationApi.Interfaces;

public interface ITranslationService
{
    Task<ListResult<SubtitleDto>> TranslateAsync(TranslationRequestDto requestDto);

    AsyncEnumerableResult<SubtitleDto> TranslateAndStreamAsync(TranslationRequestDto requestDto);
}
