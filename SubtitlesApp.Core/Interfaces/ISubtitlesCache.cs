using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Core.Interfaces;

public interface ISubtitlesCache
{
    Task<IEnumerable<Subtitle>?> Get(string key);

    Task Save(string key, IEnumerable<Subtitle> subtitles);

    void Delete(string key);
}
