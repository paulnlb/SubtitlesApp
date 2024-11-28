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
}
