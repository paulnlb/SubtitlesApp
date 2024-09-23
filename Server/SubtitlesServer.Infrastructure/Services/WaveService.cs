using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Infrastructure.Services;

public class WaveService : IWaveService
{
    public async Task<MemoryStream> WriteToWaveStreamAsync(
        IAsyncEnumerable<byte[]> dataChunks,
        TrimmedAudioMetadataDTO audioMetadata,
        CancellationToken cancellationToken = default)
    {
        MemoryStream waveStream;

        if (audioMetadata.AudioFormat == AudioFormats.PCM)
        {
            var waveFormat = new WaveFormat(audioMetadata.SampleRate, audioMetadata.ChannelsCount);

            waveStream = await WriteRawToWaveAsync(dataChunks, waveFormat, cancellationToken);
        }
        else if (audioMetadata.AudioFormat == AudioFormats.Wave)
        {
            waveStream = await WritePreprocessedWave(dataChunks, cancellationToken);
        }
        else
        {
            throw new NotSupportedException($"Audio format \"{audioMetadata.AudioFormat}\" is not supported");
        }

        Console.WriteLine("Wave read");

        if (audioMetadata.SampleRate != 16000)
        {
            waveStream.Dispose();

            waveStream = ResampleWav(waveStream, 16000);

            Console.WriteLine("Wave resampled");
        }
        return waveStream;
    }

    private static async Task<MemoryStream> WriteRawToWaveAsync(
        IAsyncEnumerable<byte[]> dataChunks,
        WaveFormat waveFormat, 
        CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();

        using var wrapperStream = new IgnoreDisposeStream(memoryStream);

        using var waveWriter = new WaveFileWriter(wrapperStream, waveFormat);

        await foreach (var chunk in dataChunks)
        {
            await waveWriter.WriteAsync(chunk, cancellationToken);
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    private static async Task<MemoryStream> WritePreprocessedWave(
        IAsyncEnumerable<byte[]> dataChunks, 
        CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();

        await foreach (var chunk in dataChunks)
        {
            await memoryStream.WriteAsync(chunk, cancellationToken);
        }

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
