using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces.Repositories;

namespace SubtitlesApp.Infrastructure.Repositories;

public class SubtitlesRepository : ISubtitlesRepository
{
    public Task Create(string key, IEnumerable<SubtitleDto> subtitles)
    {
        return Task.CompletedTask;
    }

    public Task Delete(string key)
    {
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<SubtitleDto>> Get(string key)
    {
        IEnumerable<SubtitleDto> subtitles = [];
        return subtitles;
    }
}
