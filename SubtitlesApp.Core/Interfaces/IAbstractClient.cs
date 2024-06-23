using SubtitlesApp.Core.Interfaces.Socket;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Core.Interfaces;

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
    Task SendAsync(ISocketListener socketListener, TrimmedAudioMetadata audioMetadata, CancellationToken cancellationToken = default);
}
