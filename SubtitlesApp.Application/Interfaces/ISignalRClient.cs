using SubtitlesApp.Application.Interfaces.Socket;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;

namespace SubtitlesApp.Application.Interfaces;

public interface ISignalRClient : IAbstractClient
{
    void RegisterHandler<T>(string handlerName, Action<T> handler);

    Task<(bool ConnectionResult, string ConnectionMessage)> TryConnectAsync();

    Task StopConnectionAsync();

    Task SendToHubAsync(IAsyncEnumerable<byte[]> bytesEnumerable, string hubMethodName, TrimmedAudioMetadataDTO audioMetadata, CancellationToken cancellationToken = default);
}
