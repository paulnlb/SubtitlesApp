using SubtitlesApp.Core.DTOs;

namespace SubtitlesServer.Application.Interfaces;

public interface IWaveService
{
    Task<MemoryStream> WriteToWaveStreamAsync(
        TrimmedAudioDto audioMetadata,
        CancellationToken cancellationToken = default);
}
