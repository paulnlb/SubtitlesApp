namespace SubtitlesApp.Interfaces.Socket;

/// <summary>
/// Allows receiving media data from the specified endpoint
/// </summary>
public interface ISocketListener
{
    /// <summary>
    /// Socket endpoint
    /// </summary>
    string Endpoint { get; }

    /// <summary>
    /// Start listening for media data
    /// </summary>
    void StartListening();

    /// <summary>
    /// Receive chunks of media data from the socket
    /// </summary>
    /// <returns>Async enumerable with chunks of bytes</returns>
    IAsyncEnumerable<byte[]> ReceiveAsync(int chunkSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Release all socket recources
    /// </summary>
    void Close();
}
