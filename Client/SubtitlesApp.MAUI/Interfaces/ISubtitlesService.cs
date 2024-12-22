using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface ISubtitlesService
{
    /// <summary>
    /// Send media to the server and receive subtitles
    /// </summary>
    /// <param name="audioBytes">Audio file in bytes</param>
    /// <param name="language">Language of the subtitles</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result<List<SubtitleDTO>>> GetSubsAsync(
        byte[] audioBytes,
        string languageCode,
        TimeSpan? timeOffset = null,
        CancellationToken cancellationToken = default);
}
