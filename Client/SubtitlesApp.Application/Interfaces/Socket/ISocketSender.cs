namespace SubtitlesApp.Application.Interfaces.Socket;

/// <summary>
/// Allows sending media data to the specified endpoint
/// </summary>
public interface ISocketSender
{
    /// <summary>
    /// Socket endpoint
    /// </summary>
    string Endpoint { get; }

    /// <summary>
    /// Connect to the socket
    /// </summary>
    void Connect();

    /// <summary>
    /// Signals the end of the media data writing
    /// </summary>
    void CompleteWriting();

    /// <summary>
    /// Send a chunk of media data to the socket
    /// </summary>
    /// <param name="bytes">Data chunk</param>
    /// <returns></returns>
    Task SendAsync(byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes the socket connection and allows reuse of the socket
    /// </summary>
    void Disconnect();

    /// <summary>
    /// Release all the socket recources
    /// </summary>
    void Close();
}
