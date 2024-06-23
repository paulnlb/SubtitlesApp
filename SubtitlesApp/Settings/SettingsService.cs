using SubtitlesApp.Core.Interfaces;

namespace SubtitlesApp.Settings;

public class SettingsService : ISettingsService
{
    private const string _baseUrlKey = "backend_base_url";
    private const string _baseUrl = "http://localhost:8080";

    private const string _hubAddressKey = "hub_address";
    private const string _hubAddress = "/whisperHub";

    private const string _unixSocketPathKey = "unix_socket_path";
    private const string _unixSocketPath = "media.sock";

    public string HubAddress
    {
        get => Preferences.Get(_hubAddressKey, _hubAddress);
        set => Preferences.Set(_hubAddressKey, value);
    }

    public string UnixSocketPath
    {
        get => Preferences.Get(_unixSocketPathKey, _unixSocketPath);
        set => Preferences.Set(_unixSocketPathKey, value);
    }

    public string BackendBaseUrl
    {
        get => Preferences.Get(_baseUrlKey, _baseUrl);
        set => Preferences.Set(_baseUrlKey, value);
    }
}
