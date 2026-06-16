using SubtitlesApp.Core.DTOs;

namespace SubtitlesApp.Core.Interfaces.Repositories;

public interface ISubtitlesRepository
{
    Task<IEnumerable<SubtitleDto>> Get(string key);

    Task Create(string key, IEnumerable<SubtitleDto> subtitles);

    Task Delete(string key);
}
