namespace SubtitlesApp.Interfaces;

/// <summary>
///     App settings
/// </summary>
public interface ISettingsService
{
    string UnixSocketPath { get; set; }
}
