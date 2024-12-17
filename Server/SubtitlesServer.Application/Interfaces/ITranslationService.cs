using SubtitlesApp.Core.DTOs;

namespace SubtitlesServer.Application.Interfaces;

public interface ITranslationService
{
    Task<List<SubtitleDTO>> TranslateAsync(TranslationRequestDto requestDto);
}
