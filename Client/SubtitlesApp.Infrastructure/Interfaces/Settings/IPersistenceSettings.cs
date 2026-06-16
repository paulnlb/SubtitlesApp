namespace SubtitlesApp.Infrastructure.Interfaces.Settings;

public interface IPersistenceSettings
{
    public string AppDataDirectory { get; }

    public string SubtitlesBlobsDirectory { get; }
}
