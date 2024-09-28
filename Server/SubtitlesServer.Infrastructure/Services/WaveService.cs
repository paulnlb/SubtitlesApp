using Microsoft.Extensions.Logging;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Infrastructure.Services;

public class WaveService(ILogger<WaveService> logger) : IWaveService
{
    public async Task<MemoryStream> WriteToWaveStreamAsync(
        TrimmedAudioDto audioMetadata,
        CancellationToken cancellationToken = default)
    {
        MemoryStream waveStream;

        if (audioMetadata.AudioFormat == AudioFormats.PCM)
        {
            var waveFormat = new WaveFormat(audioMetadata.SampleRate, audioMetadata.ChannelsCount);

            waveStream = await WriteRawToWaveAsync(audioMetadata.AudioBytes, waveFormat, cancellationToken);
        }
        else if (audioMetadata.AudioFormat == AudioFormats.Wave)
        {
            waveStream = await WritePreprocessedWave(audioMetadata.AudioBytes, cancellationToken);
        }
        else
        {
            throw new NotSupportedException($"Audio format \"{audioMetadata.AudioFormat}\" is not supported");
        }

        logger.LogInformation("Wave read");

        if (audioMetadata.SampleRate != 16000)
        {
            waveStream.Dispose();

            waveStream = ResampleWav(waveStream, 16000);

            logger.LogInformation("Wave resampled");
        }
        return waveStream;
    }

    private static async Task<MemoryStream> WriteRawToWaveAsync(
        byte[] audioBytes,
        WaveFormat waveFormat, 
        CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();

        using var wrapperStream = new IgnoreDisposeStream(memoryStream);

        using var waveWriter = new WaveFileWriter(wrapperStream, waveFormat);

        await waveWriter.WriteAsync(audioBytes, cancellationToken);

        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    private static async Task<MemoryStream> WritePreprocessedWave(
        byte[] audioBytes, 
        CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();

        await memoryStream.WriteAsync(audioBytes, cancellationToken);

        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    private MemoryStream ResampleWav(Stream inputStream, int samplingRate)
    {
        inputStream.Position = 0L;

        using var wrapperReadStream = new IgnoreDisposeStream(inputStream);
        using var waveReader = new WaveFileReader(wrapperReadStream);

        var sampleProvider = waveReader.ToSampleProvider();
        var resampler = new WdlResamplingSampleProvider(sampleProvider, samplingRate);

        var outputStream = new MemoryStream();

        WaveFileWriter.WriteWavFileToStream(outputStream, resampler.ToWaveProvider16());

        outputStream.Seek(0, SeekOrigin.Begin);

        return outputStream;
    }
}
