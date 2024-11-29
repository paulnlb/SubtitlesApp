using SubtitlesApp.Interfaces;

namespace SubtitlesApp.Settings;

public class SettingsService : ISettingsService
{
    private const string _baseUrlKey = "backend_base_url";
    private const string _baseUrl = "http://localhost:8080/";

    private const string _unixSocketPathKey = "unix_socket_path";
    private readonly string _unixSocketPath = Path.Combine(FileSystem.Current.AppDataDirectory, "media.sock");

    private const string _transcribeBufferLengthKey = "transcribe_buffer_length";
    private const int _transcribeBufferLength = 60;

    private const string _transcriptionPathKey = "transcription_path";
    private const string _transcriptionPath = "api/whisper/transcription";

    private const string _identityPathKey = "identity_path";
    private const string _identityPath = "identity";

    private const string _oidcClientIdKey = "oidc_client_id";
    private const string _oidcClientId = "interactive.public";

    private const string _oidcScopeKey = "oidc_scope";
    private const string _oidcScope = "openid profile api offline_access";

    private const string _oidcRedirectUriKey = "oidc_redirect_uri";
    private const string _oidcRedirectUri = "subtitlesapp://";

    private const string _oidcPostLogoutRedirectUriKey = "oidc_post_logout_redirect_uri";
    private const string _oidcPostLogoutRedirectUri = "subtitlesapp://";

    public string TranscriptionPath
    {
        get => Preferences.Get(_transcriptionPathKey, _transcriptionPath);
        set => Preferences.Set(_transcriptionPathKey, value);
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

    public string IdentityPath
    {
        get => Preferences.Get(_identityPathKey, _identityPath);
        set => Preferences.Set(_identityPathKey, value);
    }

    public string OidcClientId
    {
        get => Preferences.Get(_oidcClientIdKey, _oidcClientId);
        set => Preferences.Set(_oidcClientIdKey, value);
    }

    public string OidcScope
    {
        get => Preferences.Get(_oidcScopeKey, _oidcScope);
        set => Preferences.Set(_oidcScopeKey, value);
    }

    public string OidcRedirectUri
    {
        get => Preferences.Get(_oidcRedirectUriKey, _oidcRedirectUri);
        set => Preferences.Set(_oidcRedirectUriKey, value);
    }

    public string OidcPostLogoutRedirectUri
    {
        get => Preferences.Get(_oidcPostLogoutRedirectUriKey, _oidcPostLogoutRedirectUri);
        set => Preferences.Set(_oidcPostLogoutRedirectUriKey, value);
    }
}
