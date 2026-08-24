using SubtitlesApp.Core.DTOs;

namespace SubtitlesApp.Core.Interfaces.Repositories;

public interface IVideoSessionRepository
{
    Task<VideoSessionDto?> Get(string videoId);

    Task Create(VideoSessionDto videoSession);

    Task Update(VideoSessionDto videoSession);

    Task Delete(string videoId);

    Task DeleteMany(List<string> videoIds);

    Task<List<VideoSessionDto>> GetMany(DateTimeOffset? minModifiedOn = null, DateTimeOffset? maxModifiedOn = null);
}
