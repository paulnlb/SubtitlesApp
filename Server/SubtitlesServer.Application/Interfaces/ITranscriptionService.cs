using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.DTOs;

namespace SubtitlesServer.Application.Interfaces;

public interface ITranscriptionService
{
    Task<List<SubtitleDTO>> TranscribeAudioAsync(
        TrimmedAudioDto audioMetadata,
        CancellationToken cancellationToken = default);
}
