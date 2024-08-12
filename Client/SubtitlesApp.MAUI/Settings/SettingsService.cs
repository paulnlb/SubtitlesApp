using SubtitlesApp.Application.Interfaces;

namespace SubtitlesApp.Settings;

public class SettingsService : ISettingsService
{
    private const string _baseUrlKey = "backend_base_url";
    private const string _baseUrl = "http://localhost:8080";

    private const string _hubAddressKey = "hub_address";
    private const string _hubAddress = "/whisperHub";

    private const string _unixSocketPathKey = "unix_socket_path";
    private readonly string _unixSocketPath = Path.Combine(FileSystem.Current.AppDataDirectory, "media.sock");

    private const string _transcribeBufferLengthKey = "transcribe_buffer_length";
    private const int _transcribeBufferLength = 60;

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

    public int TranscribeBufferLength
    {
        get => Preferences.Get(_transcribeBufferLengthKey, _transcribeBufferLength);
        set => Preferences.Set(_transcribeBufferLengthKey, value);
    }
}
