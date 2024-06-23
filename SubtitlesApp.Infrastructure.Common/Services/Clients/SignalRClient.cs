using Microsoft.AspNetCore.SignalR.Client;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Application.Interfaces.Socket;

namespace SubtitlesApp.Infrastructure.Common.Services.Clients;

public class SignalRClient : ISignalRClient
{
    readonly HubConnection _connection;

    public SignalRClient(ISettingsService settings)
    {
        var hubUrl = settings.BackendBaseUrl + settings.HubAddress;

        _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .Build();

        _connection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await _connection.StartAsync();
        };
    }

    public void RegisterHandler<T>(string handlerName, Action<T> handler)
    {
        if (_connection.State != HubConnectionState.Connected)
        {
            _connection.On(handlerName, handler);
        }
        else
        {
            throw new ArgumentException("Cannot register handlers while the connection is established with a hub.");
        }
    }

    public async Task<(bool ConnectionResult, string ConnectionMessage)> TryConnectAsync()
    {
        try
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                return (false, "Already connected.");
            }
            else
            {
                await _connection.StartAsync();
                return (true, "Connected successfully.");
            }
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task StopConnectionAsync()
    {
        if (_connection.State != HubConnectionState.Disconnected)
        {
            await _connection.StopAsync();
        }
    }

    public async Task SendAsync(ISocketListener socketListener, TrimmedAudioMetadata audioMetadata, CancellationToken cancellationToken = default)
    {
        await SendToHubAsync(socketListener, "TranscribeAudio", audioMetadata, cancellationToken);
    }

    public async Task SendToHubAsync(ISocketListener socket, string hubMethodName, TrimmedAudioMetadata audioMetadata, CancellationToken cancellationToken = default)
    {
        var chunkSize = 16 * 1024;

        cancellationToken.ThrowIfCancellationRequested();

        var bytesEnumerable = socket.ReceiveAsync(chunkSize);

        await _connection.SendAsync(hubMethodName, bytesEnumerable, audioMetadata);
    }
}
