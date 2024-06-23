using SubtitlesApp.Application.Interfaces.Socket;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Application.Interfaces;

public interface ISignalRClient : IAbstractClient
{
    void RegisterHandler<T>(string handlerName, Action<T> handler);

    Task<(bool ConnectionResult, string ConnectionMessage)> TryConnectAsync();

    Task StopConnectionAsync();

    Task SendToHubAsync(ISocketListener socket, string hubMethodName, TrimmedAudioMetadata audioMetadata, CancellationToken cancellationToken = default);
}
