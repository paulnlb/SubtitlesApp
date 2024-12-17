using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface ITranslationService
{
    Task<Result<List<SubtitleDTO>>> TranslateAsync(TranslationRequestDto requestDto, CancellationToken cancellationToken = default);
}
