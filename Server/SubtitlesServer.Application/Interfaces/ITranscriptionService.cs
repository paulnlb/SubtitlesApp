using SubtitlesApp.Core.DTOs;

namespace SubtitlesServer.Application.Interfaces;

public interface ITranscriptionService
{
    Task<List<SubtitleDto>> TranscribeAudioAsync(
        byte[] audioBytes,
        string subtitlesLanguageCode,
        CancellationToken cancellationToken = default
    );
}
