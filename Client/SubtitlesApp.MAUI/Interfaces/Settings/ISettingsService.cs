namespace SubtitlesApp.Interfaces.Settings;

/// <summary>
///     App settings
/// </summary>
public interface ISettingsService
{
    string UnixSocketPath { get; set; }
}
