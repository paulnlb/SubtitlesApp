using SubtitlesApp.Shared.DTOs;

namespace SubtitlesApp.Application.Interfaces;

/// <summary>
/// Sends media to remote server
/// </summary>
public interface IAbstractClient
{
    /// <summary>
    /// Send media represented in byte chunks to the server
    /// </summary>
    /// <param name="socketListener">ISocket instance that contains data to send</param>
    /// <param name="audioMetadata">Audio metadata</param>
    /// <returns></returns>
    Task SendAsync(
        IAsyncEnumerable<byte[]> bytesEnumerable,
        TrimmedAudioMetadataDTO audioMetadata,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send media represented in byte chunks to the server and receive subtitles
    /// </summary>
    /// <param name="bytesEnumerable">Data to send</param>
    /// <param name="audioMetadata">Audio metadata</param>
    /// <returns></returns>
    IAsyncEnumerable<SubtitleDTO> StreamAsync(
        IAsyncEnumerable<byte[]> bytesEnumerable,
        TrimmedAudioMetadataDTO audioMetadata,
        CancellationToken cancellationToken = default);
}
