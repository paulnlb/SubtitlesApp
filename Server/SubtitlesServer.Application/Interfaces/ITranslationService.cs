using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesServer.Application.Interfaces;

public interface ITranslationService
{
    Task<ListResult<SubtitleDto>> TranslateAsync(TranslationRequestDto requestDto);

    AsyncEnumerableResult<SubtitleDto> TranslateAndStreamAsync(TranslationRequestDto requestDto);
}
