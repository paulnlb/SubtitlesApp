using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Interfaces;

public interface ISubtitlesService
{
    /// <summary>
    /// Send media to the server and receive subtitles
    /// </summary>
    /// <param name="audioBytes">Audio file in bytes</param>
    /// <param name="languageCode">Language of the subtitles</param>\
    /// <param name="timeOffset">Time offset</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ListResult<SubtitleDto>> GetSubsAsync(
        byte[] audioBytes,
        string languageCode,
        TimeSpan? timeOffset = null,
        CancellationToken cancellationToken = default
    );
}
