using Microsoft.Extensions.Caching.Memory;
using SubtitlesApp.Core.Models;
using SubtitlesServer.Application.Configs;
using SubtitlesServer.Application.Interfaces;
using System.Runtime.CompilerServices;
using Whisper.net;
using Whisper.net.Ggml;

namespace SubtitlesServer.Infrastructure.Services;

public class WhisperService : IWhisperService
{
    private readonly IMemoryCache _memoryCache;

    public WhisperService(
        IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        MemoryStream audioStream,
        SpeechToTextConfigs speechToTextConfigs,
        WhisperConfigs whisperConfigs,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var parsedSize = Enum.TryParse(whisperConfigs.ModelSize, out GgmlType ggmlType);
        var parsedQuantType = Enum.TryParse(whisperConfigs.QuantizationType, out QuantizationType quantizationType);

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
                Path.Combine(whisperConfigs.BinaryModelFolder, $"Whisper_{ggmlType}_{quantizationType}.bin"),
                cancellationToken);

            SetCachedModelBuffer(
                ggmlType,
                quantizationType,
                buffer,
                TimeSpan.FromDays(1));
        }

        using var whisperFactory = WhisperFactory.FromBuffer(buffer, useGpu: speechToTextConfigs.UseGpu);

        var processor = whisperFactory.CreateBuilder()
            .WithLanguage(speechToTextConfigs.Language)
            .Build();

        Console.WriteLine("Whisper loaded");

        var segments = processor.ProcessAsync(audioStream, cancellationToken);

        Console.WriteLine("Starting transcribing...");
        
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
