using SubtitlesApp.Infrastructure.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class PersistenceSettings : IPersistenceSettings
{
    public string AppDataDirectory
    {
        get => FileSystem.AppDataDirectory;
    }

    public string SubtitlesBlobsDirectory => "subtitles";
}
