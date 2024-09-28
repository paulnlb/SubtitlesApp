using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SubtitlesApp.Core.Models;
using SubtitlesServer.Application.Configs;
using SubtitlesServer.Application.Interfaces;
using System.Runtime.CompilerServices;
using Whisper.net;
using Whisper.net.Ggml;

namespace SubtitlesServer.Infrastructure.Services;

public class WhisperService : IWhisperService
{
    private readonly SpeechToTextConfigs _speechToTextConfigs;
    private readonly WhisperConfigs _whisperConfigs;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;

    public WhisperService(
        ILogger<WhisperService> logger,
        IMemoryCache memoryCache,
        IOptions<SpeechToTextConfigs> speechToTextConfig,
        IOptions<WhisperConfigs> whisperConfigs)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _speechToTextConfigs = speechToTextConfig.Value;
        _whisperConfigs = whisperConfigs.Value;
    }

    public async IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        MemoryStream audioStream,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var parsedSize = Enum.TryParse(_whisperConfigs.ModelSize, out GgmlType ggmlType);
        var parsedQuantType = Enum.TryParse(_whisperConfigs.QuantizationType, out QuantizationType quantizationType);

        if (!parsedSize || !parsedQuantType)
        {
            throw new ArgumentException("Invalid Whisper model size or quantization type");
        }

        var buffer = GetCachedModelBuffer(
            ggmlType,
            quantizationType);

        if (buffer is null)
        {
            buffer = await ReadModelBuffer(
                ggmlType,
                quantizationType,
                Path.Combine(_whisperConfigs.BinaryModelFolder, $"Whisper_{ggmlType}_{quantizationType}.bin"),
                cancellationToken);

            SetCachedModelBuffer(
                ggmlType,
                quantizationType,
                buffer,
                TimeSpan.FromDays(1));
        }

        using var whisperFactory = WhisperFactory.FromBuffer(buffer, useGpu: _speechToTextConfigs.UseGpu);

        var processor = whisperFactory.CreateBuilder()
            .WithLanguage(_speechToTextConfigs.Language)
        .Build();

        _logger.LogInformation("Whisper loaded");

        var segments = processor.ProcessAsync(audioStream, cancellationToken);

        _logger.LogInformation("Starting transcribing...");
        
        try
        {
            await foreach (var result in segments)
            {
                var subtitle = new Subtitle()
                {
                    Text = result.Text,
                    TimeInterval = new TimeInterval(result.Start, result.End),
                };

                yield return subtitle;
            }
        }
        finally
        {
            await processor.DisposeAsync();
        }
    }

    private byte[]? GetCachedModelBuffer(
        GgmlType ggmlType,
        QuantizationType quantizationType)
    {
        var key = $"Whisper_{ggmlType}_{quantizationType}";

        _memoryCache.TryGetValue(key, out byte[]? buffer);

        return buffer;
    }

    private async Task<byte[]> ReadModelBuffer(
        GgmlType ggmlType,
        QuantizationType quantizationType,
        string modelPath,
        CancellationToken cancellationToken)
    {
        // whisper load
        if (!File.Exists(modelPath))
        {
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(
                ggmlType,
                quantizationType,
                cancellationToken);

            using var fileWriter = File.OpenWrite(modelPath);
            await modelStream.CopyToAsync(fileWriter, cancellationToken);
        }

        return File.ReadAllBytes(modelPath);
    }

    private void SetCachedModelBuffer(
        GgmlType ggmlType,
        QuantizationType quantizationType,
        byte[] buffer,
        TimeSpan cacheDuration)
    {
        var key = $"Whisper_{ggmlType}_{quantizationType}";

        _memoryCache.Set(key, buffer, cacheDuration);
    }
}
