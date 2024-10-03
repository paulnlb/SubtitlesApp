using Microsoft.Extensions.Options;
using SubtitlesServer.Infrastructure.Configs;
using Whisper.net;
using Whisper.net.Ggml;

namespace SubtitlesServer.Infrastructure.Services;

public sealed class WhisperModelService : IDisposable
{
    private readonly WhisperConfigs _whisperConfigs;
    private readonly SpeechToTextConfigs _speechToTextConfigs;
    private readonly Lazy<Task<WhisperFactory>> _whisperFactoryTask;

    private bool _disposed = false;

    public WhisperModelService(
        IOptions<WhisperConfigs> whisperConfigs,
        IOptions<SpeechToTextConfigs> speechToTextConfigs)
    {
        _whisperConfigs = whisperConfigs.Value;
        _speechToTextConfigs = speechToTextConfigs.Value;

        _whisperFactoryTask = new Lazy<Task<WhisperFactory>>(() =>
            GetFactoryAsync(
                _whisperConfigs.ModelSize,
                _whisperConfigs.QuantizationType,
                _whisperConfigs.BinaryModelFolder,
                _speechToTextConfigs.UseGpu)
        );
    }

    public Task<WhisperFactory> GetWhisperFactoryAsync()
    {
        return _whisperFactoryTask.Value;
    }

    private static async Task<WhisperFactory> GetFactoryAsync(
        GgmlType ggmlType,
        QuantizationType quantizationType,
        string binaryModelsFolder,
        bool useGpu,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(binaryModelsFolder, $"Whisper_{ggmlType}_{quantizationType}.bin");

        if (!File.Exists(fullPath))
        {
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(
                ggmlType,
                quantizationType,
                cancellationToken);

            using var fileWriter = File.OpenWrite(fullPath);
            await modelStream.CopyToAsync(fileWriter, cancellationToken);
        }

        return WhisperFactory.FromPath(fullPath, useGpu: useGpu);
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


