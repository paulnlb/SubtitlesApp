using SubtitlesApp.Core.Models;
using SubtitlesServer.Application.Configs;

namespace SubtitlesServer.Application.Interfaces;

public interface IWhisperService
{
    IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        MemoryStream audioStream,
        SpeechToTextConfigs speechToTextConfigs,
        WhisperConfigs whisperConfigs,
        CancellationToken cancellationToken = default);
}
