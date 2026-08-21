using SQLite;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces.Repositories;
using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Mapper;
using SubtitlesApp.Infrastructure.Models;

namespace SubtitlesApp.Infrastructure.Repositories;

public class VideoSessionRepository(IPersistenceSettings persistenceSettings) : IVideoSessionRepository
{
    SQLiteAsyncConnection _database;

    async Task Init()
    {
        if (_database is not null)
            return;

        _database = new SQLiteAsyncConnection(
            Path.Combine(persistenceSettings.AppDataDirectory, SqliteConstants.DatabaseFilename),
            SqliteConstants.Flags
        );
        await _database.CreateTableAsync<VideoSessionEntity>();
    }

    public async Task Create(VideoSessionDto videoSession)
    {
        await Init();

        var entity = VideoSessionMapper.ToEntity(videoSession);
        entity.ModifiedOn = DateTimeOffset.UtcNow;

        await _database.InsertAsync(entity);
    }

    public async Task Delete(string videoId)
    {
        await Init();

        await _database.DeleteAsync<VideoSessionEntity>(videoId);
    }

    public async Task DeleteMany(List<string> videoIds)
    {
        await Init();

        await _database.Table<VideoSessionEntity>().DeleteAsync(x => videoIds.Contains(x.VideoId));
    }

    public async Task<VideoSessionDto?> Get(string videoId)
    {
        await Init();

        var entity = await _database.Table<VideoSessionEntity>().Where(x => x.VideoId == videoId).FirstOrDefaultAsync();

        if (entity is null)
        {
            return null;
        }

        return VideoSessionMapper.ToDto(entity);
    }

    public async Task<List<VideoSessionDto>> GetMany(
        DateTimeOffset? minModifiedOn = null,
        DateTimeOffset? maxModifiedOn = null
    )
    {
        await Init();

        var query = _database.Table<VideoSessionEntity>();

        if (minModifiedOn is not null)
        {
            query = query.Where(x => x.ModifiedOn >= minModifiedOn);
        }
        if (maxModifiedOn is not null)
        {
            query = query.Where(x => x.ModifiedOn <= maxModifiedOn);
        }

        var result = await query.ToListAsync();

        return VideoSessionMapper.ToDtos(result).ToList();
    }

    public async Task Update(VideoSessionDto videoSession)
    {
        await Init();

        var entity = VideoSessionMapper.ToEntity(videoSession);
        entity.ModifiedOn = DateTimeOffset.UtcNow;

        await _database.UpdateAsync(entity);
    }
}
