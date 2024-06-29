using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Application.Interfaces.Socket;
using System.Net.Sockets;

namespace SubtitlesApp.Infrastructure.Common.Services.Sockets;

public class UnixSocketSender : ISocketSender
{
    readonly string _endpoint;
    readonly Socket _udSocket;

    bool _isCurrentlyWriting = false;
    bool _connected = false;

    public UnixSocketSender(ISettingsService settings)
    {
        _endpoint = settings.UnixSocketPath;

        _udSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
    }

    public string Endpoint => _endpoint;

    public void Close()
    {
        if (_isCurrentlyWriting)
        {
            throw new InvalidOperationException("Cannot close while writing");
        }

        _udSocket.Close();

        _connected = false;
    }

    public void CompleteWriting()
    {
        _udSocket.Shutdown(SocketShutdown.Send);

        _isCurrentlyWriting = false;
    }

    public void Connect()
    {
        var udsEndpointObject = new UnixDomainSocketEndPoint(_endpoint);
        _udSocket.Connect(udsEndpointObject);

        _connected = true;
    }

    public void Disconnect()
    {
        if (_isCurrentlyWriting)
        {
            throw new InvalidOperationException("Cannot disconnect while writing");
        }

        if (_connected)
        {
            _udSocket.Disconnect(true);
        }
    }

    public async Task SendAsync(byte[] bytes, CancellationToken cancellationToken = default)
    {
        if (!_connected)
        {
            throw new InvalidOperationException("Socket is not connected");
        }
        try
        {
            await SendBytesAsync(bytes, cancellationToken);
        }
        catch (OperationCanceledException ex)
        {
            _isCurrentlyWriting = false;

            throw new OperationCanceledException("Send operation was cancelled", ex);
        }

        _isCurrentlyWriting = true;
    }

    private async Task SendBytesAsync(byte[] bytes, CancellationToken cancellationToken)
    {
        int bytesSent = 0;

        while (bytesSent < bytes.Length)
        {
            bytesSent += await _udSocket.SendAsync(
                new ArraySegment<byte>(bytes, bytesSent, bytes.Length - bytesSent),
                SocketFlags.None,
                cancellationToken);
        }
    }
}
