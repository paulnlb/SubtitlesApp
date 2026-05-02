using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Interfaces;

public interface ITranscriptionService : IDisposable
{
    /// <summary>
    ///     Extracts a part within given time interval from given media and transcribes it (generates subtitles)
    /// </summary>
    /// <param name="mediaPath">Physical path for media to transcribe</param>
    /// <param name="timeInterval">Time interval that should be taken from given media and transcribed</param>
    /// <param name="languageCode">Language code of the language in which the media is spoken</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Result of transcription. If the result is successfull, it will contain a list of transcribed subtitles</returns>
    IAsyncEnumerable<Result<SubtitleDto>> TranscribeAsync(
        string mediaPath,
        TimeInterval timeInterval,
        string languageCode,
        CancellationToken cancellationToken = default
    );
}
