using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Interfaces.HttpClients;

public interface ITranscriptionApiClient
{
    /// <summary>
    /// Send media to the server and receive subtitles
    /// </summary>
    /// <param name="audio">Audio stream</param>
    /// <param name="languageCode">Language of the subtitles</param>\
    /// <param name="cancellationToken">Cancellation token</param>
    Task<ListResult<SubtitleDto>> GetSubsAsync(
        Stream audio,
        string languageCode,
        CancellationToken cancellationToken = default
    );
}
