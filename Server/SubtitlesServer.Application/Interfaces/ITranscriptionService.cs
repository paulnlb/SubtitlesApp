using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Configs;


namespace SubtitlesServer.Application.Interfaces;

public interface ITranscriptionService
{
    IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        IAsyncEnumerable<byte[]> dataChunks,
        TrimmedAudioMetadataDTO audioMetadata,
        SpeechToTextConfigs speechToTextConfigs,
        WhisperConfigs whisperConfigs,
        CancellationToken cancellationToken = default);
}
