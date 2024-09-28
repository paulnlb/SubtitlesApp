using SubtitlesApp.Core.DTOs;

namespace SubtitlesApp.Interfaces;

public interface ISubtitlesService
{
    /// <summary>
    /// Send media to the server and receive subtitles
    /// </summary>
    /// <param name="audioMetadata">Audio metadata</param>
    /// <returns></returns>
    Task<List<SubtitleDTO>> GetSubsAsync(
        TrimmedAudioDto audioMetadata,
        CancellationToken cancellationToken = default);
}
