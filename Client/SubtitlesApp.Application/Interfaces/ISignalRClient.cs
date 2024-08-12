using SubtitlesApp.Shared.DTOs;

namespace SubtitlesApp.Application.Interfaces;

public interface ISignalRClient : IAbstractClient
{
    void RegisterHandler<T>(string handlerName, Action<T> handler);

    void RegisterHandler<T1, T2>(string handlerName, Action<T1, T2> handler);

    Task<(bool ConnectionResult, string ConnectionMessage)> TryConnectAsync();

    Task StopConnectionAsync();

    Task SendToHubAsync(IAsyncEnumerable<byte[]> bytesEnumerable, string hubMethodName, TrimmedAudioMetadataDTO audioMetadata, CancellationToken cancellationToken = default);

    void CancelTranscription();
}
