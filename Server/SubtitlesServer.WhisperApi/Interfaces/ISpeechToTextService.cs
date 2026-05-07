using SubtitlesServer.Shared.Models;
using SubtitlesServer.WhisperApi.Models;

namespace SubtitlesServer.WhisperApi.Interfaces;

public interface ISpeechToTextService
{
    IAsyncEnumerable<SubtitleDto> TranscribeAudioAsync(
        TranscriptionRequestDto whisperDto,
        CancellationToken cancellationToken = default
    );
}
