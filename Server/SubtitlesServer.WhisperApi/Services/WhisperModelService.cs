using Microsoft.Extensions.Options;
using SubtitlesServer.WhisperApi.Configs;
using Whisper.net;
using Whisper.net.Ggml;

namespace SubtitlesServer.WhisperApi.Services;

public sealed class WhisperModelService : IDisposable
{
    private readonly WhisperConfig _whisperConfigs;
    private readonly Lazy<Task<WhisperFactory>> _whisperFactoryTask;
    private readonly ILogger _logger;

    private bool _disposed = false;

    public WhisperModelService(
        IOptions<WhisperConfig> whisperConfigs,
        IHostApplicationLifetime applicationLifetime,
        ILogger<WhisperModelService> logger
    )
    {
        _whisperConfigs = whisperConfigs.Value;
        _logger = logger;

        _whisperFactoryTask = new Lazy<Task<WhisperFactory>>(
            () =>
                GetFactoryAsync(
                    _whisperConfigs.ModelSize,
                    _whisperConfigs.QuantizationType,
                    _whisperConfigs.BinaryModelFolder
                )
        );

        applicationLifetime.ApplicationStopping.Register(Dispose);
    }

    public Task<WhisperFactory> GetWhisperFactoryAsync()
    {
        return _whisperFactoryTask.Value;
    }

    private async Task<WhisperFactory> GetFactoryAsync(
        GgmlType ggmlType,
        QuantizationType quantizationType,
        string binaryModelsFolder,
        CancellationToken cancellationToken = default
    )
    {
        var modelName = $"Whisper_{ggmlType}_{quantizationType}.bin";
        var fullPath = Path.Combine(binaryModelsFolder, modelName);

        if (!File.Exists(fullPath))
        {
            _logger.LogInformation("{modelName} not found locally. Downloading...", modelName);

            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(
                ggmlType,
                quantizationType,
                cancellationToken
            );

            using var fileWriter = File.OpenWrite(fullPath);
            await modelStream.CopyToAsync(fileWriter, cancellationToken);
        }

        _logger.LogInformation("Loading Whisper into memory...");

        return WhisperFactory.FromPath(fullPath);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_whisperFactoryTask.IsValueCreated)
        {
            _whisperFactoryTask.Value.Result?.Dispose();
        }

        _disposed = true;
    }
}
