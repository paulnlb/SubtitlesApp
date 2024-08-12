using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;


namespace SubtitlesServer.Application.Interfaces;

public interface ITranscriptionService
{
    IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        IAsyncEnumerable<byte[]> dataChunks,
        TrimmedAudioMetadataDTO audioMetadata,
        CancellationToken cancellationToken = default);
}
