namespace SubtitlesApp.Core.Interfaces.Socket;

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
    /// Send a chunk of media data to the socket
    /// </summary>
    /// <param name="bytes">Data chunk</param>
    /// <returns></returns>
    Task SendAsync(byte[] bytes);

    /// <summary>
    /// Signal that write operation is completed
    /// </summary>
    void CompleteWrite();

    /// <summary>
    /// Release all the socket recources
    /// </summary>
    void Close();
}
