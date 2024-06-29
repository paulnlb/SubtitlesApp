﻿using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Shared.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Infrastructure.Services;

public class WaveService : IWaveService
{
    public async Task<MemoryStream> WriteToWaveStreamAsync(IAsyncEnumerable<byte[]> dataChunks, TrimmedAudioMetadataDTO audioMetadata, CancellationToken cancellationToken)
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
            var resampledWav = await ResampleWavAsync(waveStream, 16000);

            Console.WriteLine("Wave resampled");
            waveStream.Dispose();
            return resampledWav;
        }
        return waveStream;
    }

    private async Task<MemoryStream> WriteRawToWaveAsync(IAsyncEnumerable<byte[]> dataChunks, WaveFormat waveFormat, CancellationToken cancellationToken)
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

    private async Task<MemoryStream> WritePreprocessedWave(IAsyncEnumerable<byte[]> dataChunks, CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();

        await foreach (var chunk in dataChunks)
        {
            await memoryStream.WriteAsync(chunk, cancellationToken);
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    private async Task<MemoryStream> ResampleWavAsync(Stream inputStream, int samplingRate)
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
