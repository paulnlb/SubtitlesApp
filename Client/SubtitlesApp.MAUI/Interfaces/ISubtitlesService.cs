using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface ISubtitlesService
{
    /// <summary>
    /// Send media to the server and receive subtitles
    /// </summary>
    /// <param name="audioMetadata">Audio metadata</param>
    /// <returns></returns>
    Task<Result<List<SubtitleDTO>>> GetSubsAsync(
        TrimmedAudioDto audioMetadata,
        CancellationToken cancellationToken = default);
}
