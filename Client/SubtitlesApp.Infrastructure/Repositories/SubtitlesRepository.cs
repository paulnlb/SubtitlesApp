using MessagePack;
using SubtitlesApp.Core.Interfaces.Repositories;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Infrastructure.DataModels;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Mapper;

namespace SubtitlesApp.Infrastructure.Repositories;

public class SubtitlesRepository : ISubtitlesRepository
{
    private readonly string SubtitlesDirectory;

    public SubtitlesRepository(IPersistenceSettings persistenceSettings)
    {
        SubtitlesDirectory = Path.Combine(persistenceSettings.AppDataDirectory, persistenceSettings.SubtitlesBlobsDirectory);
        Directory.CreateDirectory(SubtitlesDirectory);
    }

    public async Task Create(string key, IEnumerable<Subtitle> subtitles)
    {
        var filePath = Path.Combine(SubtitlesDirectory, key);
        var serializables = SubtitleMapper.ToSerializables(subtitles);

        using var fileStream = File.OpenWrite(filePath);

        await MessagePackSerializer.SerializeAsync(fileStream, serializables);
    }

    public void Delete(string key)
    {
        var filePath = Path.Combine(SubtitlesDirectory, key);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public async Task<IEnumerable<Subtitle>> Get(string key)
    {
        var filePath = Path.Combine(SubtitlesDirectory, key);
        if (!File.Exists(filePath))
        {
            return [];
        }

        using var fileStream = File.OpenRead(filePath);
        var serializables = await MessagePackSerializer.DeserializeAsync<IEnumerable<SubtitleSerializable>>(fileStream);

        return SubtitleMapper.ToDomainClasses(serializables);
    }
}
