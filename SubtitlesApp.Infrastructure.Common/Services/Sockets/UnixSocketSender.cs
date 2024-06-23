using SubtitlesApp.Core.Interfaces;
using SubtitlesApp.Core.Interfaces.Socket;
using System.Net.Sockets;

namespace SubtitlesApp.Infrastructure.Common.Services.Sockets;

public class UnixSocketSender : ISocketSender
{
    readonly string _endpoint;
    readonly Socket _udSocket;

    public UnixSocketSender(ISettingsService settings)
    {
        _endpoint = settings.UnixSocketPath;

        _udSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
    }

    public string Endpoint => _endpoint;

    public void Close()
    {
        _udSocket.Shutdown(SocketShutdown.Both);
        _udSocket.Close();
    }

    public void CompleteWrite()
    {
        _udSocket.Shutdown(SocketShutdown.Send);
    }

    public void Connect()
    {
        var udsEndpointObject = new UnixDomainSocketEndPoint(_endpoint);
        _udSocket.Connect(udsEndpointObject);
    }

    public async Task SendAsync(byte[] bytes)
    {
        int bytesSent = 0;
        while (bytesSent < bytes.Length)
        {
            bytesSent += await _udSocket.SendAsync(new ArraySegment<byte>(bytes, bytesSent, bytes.Length - bytesSent), SocketFlags.None);
        }
    }
}
