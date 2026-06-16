using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Infrastructure.DataModels;

namespace SubtitlesApp.Infrastructure.Mapper;

public static class VideoSessionMapper
{
    public static VideoSessionEntity ToEntity(VideoSessionDto dto)
    {
        return new VideoSessionEntity
        {
            VideoId = dto.VideoId,
            PlaybackPosition = dto.PlaybackPosition,
            SubtitlesReference = dto.SubtitlesReference,
            TranslationsReference = dto.TranslationsReference,
        };
    }

    public static VideoSessionDto ToDto(VideoSessionEntity entity)
    {
        return new VideoSessionDto
        {
            VideoId = entity.VideoId,
            PlaybackPosition = entity.PlaybackPosition,
            SubtitlesReference = entity.SubtitlesReference,
            TranslationsReference = entity.TranslationsReference,
        };
    }

    public static IEnumerable<VideoSessionDto> ToDtos(IEnumerable<VideoSessionEntity> entities)
    {
        return entities.Select(x => new VideoSessionDto
        {
            VideoId = x.VideoId,
            PlaybackPosition = x.PlaybackPosition,
            SubtitlesReference = x.SubtitlesReference,
            TranslationsReference = x.TranslationsReference,
        });
    }
}
