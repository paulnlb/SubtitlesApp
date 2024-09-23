namespace SubtitlesApp.Maui.Interfaces;

/// <summary>
///     App settings
/// </summary>
public interface ISettingsService
{
    string BackendBaseUrl { get; set; }

    string HubAddress { get; set; }

    string UnixSocketPath { get; set; }

    int TranscribeBufferLength { get; set; }
}
