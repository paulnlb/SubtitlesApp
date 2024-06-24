using Microsoft.AspNetCore.SignalR.Client;
using SubtitlesApp.Application.Interfaces;
using SubtitlesApp.Shared.DTOs;

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

    public async Task SendAsync(IAsyncEnumerable<byte[]> bytesEnumerable, TrimmedAudioMetadataDTO audioMetadata, CancellationToken cancellationToken = default)
    {
        await SendToHubAsync(bytesEnumerable, "TranscribeAudio", audioMetadata, cancellationToken);
    }

    public async Task SendToHubAsync(IAsyncEnumerable<byte[]> bytesEnumerable, string hubMethodName, TrimmedAudioMetadataDTO audioMetadata, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _connection.SendAsync(hubMethodName, bytesEnumerable, audioMetadata);
    }
}
