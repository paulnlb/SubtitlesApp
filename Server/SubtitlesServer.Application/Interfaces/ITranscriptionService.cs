using SubtitlesApp.Core.DTOs;

namespace SubtitlesServer.Application.Interfaces;

public interface ITranscriptionService
{
    Task<List<SubtitleDTO>> TranscribeAudioAsync(
        byte[] audioBytes,
        string subtitlesLanguageCode,
        CancellationToken cancellationToken = default);
}
