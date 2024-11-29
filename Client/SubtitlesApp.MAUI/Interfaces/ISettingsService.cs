namespace SubtitlesApp.Interfaces;

/// <summary>
///     App settings
/// </summary>
public interface ISettingsService
{
    string BackendBaseUrl { get; set; }

    string TranscriptionPath { get; set; }

    string UnixSocketPath { get; set; }

    int TranscribeBufferLength { get; set; }

    string IdentityPath { get; set; }

    string OidcClientId { get; set; }

    string OidcScope { get; set; }

    string OidcRedirectUri { get; set; }

    string OidcPostLogoutRedirectUri { get; set; }
}
