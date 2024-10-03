using SubtitlesApp.Interfaces;
using SubtitlesApp.Interfaces.Socket;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Socket = System.Net.Sockets.Socket;

namespace SubtitlesApp.Services.Sockets;

public class UnixSocketListener : ISocketListener
{
    readonly string _endpoint;
    readonly Socket _udSocket;

    bool _isListening = false;

    public UnixSocketListener(ISettingsService settings)
    {
        _endpoint = settings.UnixSocketPath;

        if (File.Exists(_endpoint)) File.Delete(_endpoint);

        _udSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
    }

    public string Endpoint => _endpoint;

    public void Close()
    {
        _udSocket.Shutdown(SocketShutdown.Both);
        _udSocket.Close();
    }

    public void StartListening()
    {
        var udsEndpointObject = new UnixDomainSocketEndPoint(_endpoint);

        _udSocket.Bind(udsEndpointObject);

        _udSocket.Listen(5);

        _isListening = true;
    }

    public async IAsyncEnumerable<byte[]> ReceiveAsync(int chunkSize, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_isListening)
        {
            throw new InvalidOperationException("Socket is not listening");
        }

        var acceptedSocket = await _udSocket.AcceptAsync(cancellationToken);

        byte[] buffer = new byte[chunkSize];

        int bytesRead;

        try
        {
            while ((bytesRead =
                await acceptedSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    SocketFlags.None,
                    cancellationToken)) > 0)
            {
                byte[] chunk = new byte[bytesRead];
                Array.Copy(buffer, chunk, bytesRead);
                yield return chunk;
            }
        }

        finally
        {
            acceptedSocket.Shutdown(SocketShutdown.Both);
            acceptedSocket.Close();
        }
    }
}
