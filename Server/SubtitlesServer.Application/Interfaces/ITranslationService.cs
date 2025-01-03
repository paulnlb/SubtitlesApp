using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesServer.Application.Interfaces;

public interface ITranslationService
{
    Task<ListResult<SubtitleDTO>> TranslateAsync(TranslationRequestDto requestDto);
}
