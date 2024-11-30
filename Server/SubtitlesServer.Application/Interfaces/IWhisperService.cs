using SubtitlesApp.Core.Models;

namespace SubtitlesServer.Application.Interfaces;

public interface IWhisperService
{
    IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        byte[] audioBytes,
        string subtitlesLanguageCode,
        CancellationToken cancellationToken = default);
}
