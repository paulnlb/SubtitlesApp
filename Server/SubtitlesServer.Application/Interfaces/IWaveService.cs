using SubtitlesApp.Core.DTOs;

namespace SubtitlesServer.Application.Interfaces;

public interface IWaveService
{
    Task<MemoryStream> WriteToWaveStreamAsync(
        IAsyncEnumerable<byte[]> dataChunks,
        TrimmedAudioMetadataDTO audioMetadata,
        CancellationToken cancellationToken = default);
}
