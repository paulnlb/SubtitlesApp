using MessagePack;
using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Infrastructure.Interfaces.Settings;
using SubtitlesApp.Infrastructure.Mapper;
using SubtitlesApp.Infrastructure.Models;

namespace SubtitlesApp.Infrastructure.Services;

public class SubtitlesCache : ISubtitlesCache
{
    private readonly string SubtitlesDirectory;

    public SubtitlesCache(IPersistenceSettings persistenceSettings)
    {
        SubtitlesDirectory = Path.Combine(persistenceSettings.AppDataDirectory, persistenceSettings.SubtitlesBlobsDirectory);
        Directory.CreateDirectory(SubtitlesDirectory);
    }

    public async Task Save(string key, IEnumerable<Subtitle> subtitles)
    {
        var filePath = Path.Combine(SubtitlesDirectory, key);
        var serializables = SubtitleMapper.ToSerializables(subtitles);

        if (File.Exists(filePath))
        {
            using var memoryStream = new MemoryStream();
            await MessagePackSerializer.SerializeAsync(memoryStream, serializables);
            await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
        }
        else
        {
            using var fileStream = File.OpenWrite(filePath);
            await MessagePackSerializer.SerializeAsync(fileStream, serializables);
        }
    }

    public void Delete(string key)
    {
        var filePath = Path.Combine(SubtitlesDirectory, key);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public async Task<IEnumerable<Subtitle>?> Get(string key)
    {
        var filePath = Path.Combine(SubtitlesDirectory, key);

        if (!File.Exists(filePath))
        {
            return null;
        }

        using var fileStream = File.OpenRead(filePath);
        var serializables = await MessagePackSerializer.DeserializeAsync<IEnumerable<SubtitleSerializable>>(fileStream);

        return SubtitleMapper.ToDomainClasses(serializables);
    }
}
