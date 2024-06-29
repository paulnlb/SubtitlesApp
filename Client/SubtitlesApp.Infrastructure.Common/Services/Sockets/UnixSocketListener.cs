using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Application.Interfaces.Socket;
using System.Net.Sockets;
using Socket = System.Net.Sockets.Socket;

namespace SubtitlesApp.Infrastructure.Common.Services.Sockets;

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

    public async IAsyncEnumerable<byte[]> ReceiveAsync(int chunkSize)
    {
        if (!_isListening)
        {
            throw new InvalidOperationException("Socket is not listening");
        }

        var acceptedSocket = await _udSocket.AcceptAsync();

        byte[] buffer = new byte[chunkSize];

        int bytesRead;
        while ((bytesRead = await acceptedSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None)) > 0)
        {
            byte[] chunk = new byte[bytesRead];
            Array.Copy(buffer, chunk, bytesRead);
            yield return chunk;
        }
    }
}
