using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Core.Interfaces.Repositories;

public interface ISubtitlesRepository
{
    Task<IEnumerable<Subtitle>> Get(string key);

    Task Create(string key, IEnumerable<Subtitle> subtitles);

    void Delete(string key);
}
