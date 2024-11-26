using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Settings;

public class SettingsService : ISettingsService
{
    private const string _baseUrlKey = "backend_base_url";
    private const string _baseUrl = "http://localhost:8080/api/whisper/";

    private const string _whisperAddressKey = "whisper_address";
    private const string _whisperAddress = "transcription";

    private const string _unixSocketPathKey = "unix_socket_path";
    private readonly string _unixSocketPath = Path.Combine(FileSystem.Current.AppDataDirectory, "media.sock");

    private const string _transcribeBufferLengthKey = "transcribe_buffer_length";
    private const int _transcribeBufferLength = 60;

    private const string _callBackUrlKey = "callback_url";
    private const string _callBackUrl = "subtitlesapp://";

    public string WhisperAddress
    {
        get => Preferences.Get(_whisperAddressKey, _whisperAddress);
        set => Preferences.Set(_whisperAddressKey, value);
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

    public string CallBackUrl
    {
        get => Preferences.Get(_callBackUrlKey, _callBackUrl);
        set => Preferences.Set(_callBackUrlKey, value);
    }
}
