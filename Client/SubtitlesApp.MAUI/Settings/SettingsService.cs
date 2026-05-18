using SubtitlesApp.Interfaces.Settings;

namespace SubtitlesApp.Settings;

public class SettingsService : ISettingsService
{
    private const string _unixSocketPathKey = "unix_socket_path";
    private readonly string _unixSocketPath = Path.Combine(FileSystem.Current.AppDataDirectory, "media.sock");

    public string UnixSocketPath
    {
        get => Preferences.Get(_unixSocketPathKey, _unixSocketPath);
        set => Preferences.Set(_unixSocketPathKey, value);
    }
}
