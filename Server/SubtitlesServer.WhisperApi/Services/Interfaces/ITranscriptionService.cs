using SubtitlesApp.Core.DTOs;
using SubtitlesServer.WhisperApi.Models;

namespace SubtitlesServer.WhisperApi.Services.Interfaces;

public interface ITranscriptionService
{
    Task<List<SubtitleDto>> TranscribeAudioAsync(
        WhisperRequestModel whisperRequestModel,
        CancellationToken cancellationToken = default
    );
}
