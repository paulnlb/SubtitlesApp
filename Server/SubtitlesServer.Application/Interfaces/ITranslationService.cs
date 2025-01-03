using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesServer.Application.Interfaces;

public interface ITranslationService
{
    Task<Result<List<SubtitleDTO>>> TranslateAsync(TranslationRequestDto requestDto);
}
