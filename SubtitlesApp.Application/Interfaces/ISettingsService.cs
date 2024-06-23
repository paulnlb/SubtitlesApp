namespace SubtitlesApp.Application.Interfaces;

/// <summary>
///     App settings
/// </summary>
public interface ISettingsService
{
    string BackendBaseUrl { get; set; }

    string HubAddress { get; set; }

    string UnixSocketPath { get; set; }
}
