using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces.Repositories;

namespace SubtitlesApp.Infrastructure.Repositories;

public class VideoSessionRepository : IVideoSessionRepository
{
    public Task Create(VideoSessionDto videoSession)
    {
        return Task.CompletedTask;
    }

    public Task Delete(string videoId)
    {
        return Task.CompletedTask;
    }

    public Task<VideoSessionDto?> Get(string videoId)
    {
        VideoSessionDto? videoSession = null;
        return Task.FromResult(videoSession);
    }

    public Task Update(VideoSessionDto videoSession)
    {
        return Task.CompletedTask;
    }
}
