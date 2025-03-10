using SubtitlesApp.Core.DTOs;
using SubtitlesServer.WhisperApi.Models;

namespace SubtitlesServer.WhisperApi.Interfaces;

public interface ITranscriptionService
{
    Task<List<SubtitleDto>> TranscribeAudioAsync(
        TranscriptionRequestModel whisperRequestModel,
        CancellationToken cancellationToken = default
    );
}
