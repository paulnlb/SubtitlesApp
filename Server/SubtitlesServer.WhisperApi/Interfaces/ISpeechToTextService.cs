using SubtitlesApp.Core.DTOs;
using SubtitlesServer.WhisperApi.Models;

namespace SubtitlesServer.WhisperApi.Interfaces;

public interface ISpeechToTextService
{
    IAsyncEnumerable<SubtitleDto> TranscribeAudioAsync(WhisperDto whisperDto, CancellationToken cancellationToken = default);
}
