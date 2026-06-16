using SQLite;
using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Interfaces.Repositories;
using SubtitlesApp.Infrastructure.Constants;
using SubtitlesApp.Infrastructure.DataModels;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Mapper;

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
        await _database.InsertAsync(entity);
    }

    public async Task Delete(string videoId)
    {
        await Init();
        var entity =
            await _database.Table<VideoSessionEntity>().Where(x => x.VideoId == videoId).FirstOrDefaultAsync()
            ?? throw new InvalidOperationException($"Item with id {videoId} does not exist");

        await _database.DeleteAsync(entity);
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

    public async Task Update(VideoSessionDto videoSession)
    {
        await Init();

        var entity = VideoSessionMapper.ToEntity(videoSession);

        await _database.UpdateAsync(entity);
    }
}
