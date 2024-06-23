using Microsoft.AspNetCore.SignalR;
using NAudio.Utils;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SubtitlesApp.Core.Constants;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;
using Whisper.net;
using Whisper.net.Ggml;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedSingleton<IWaveService, WaveService>("wave");

builder.Services.AddRazorPages();
builder.Services.AddSignalR(
    options => options.MaximumReceiveMessageSize = 40000);

var app = builder.Build();

app.MapRazorPages();
app.MapHub<MyHub>("/whisperHub");

app.Run();

public class Subtitle
{
    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public string Text { get; set; }
}

public interface IWaveService
{
    Task<MemoryStream> WriteToWaveStreamAsync(IAsyncEnumerable<byte[]> dataChunks, TrimmedAudioMetadataDTO audioMetadata, CancellationToken cancellationToken);
}
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

            Console.WriteLine($"Received: {chunk.Length} bytes.");
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    private async Task<MemoryStream> WritePreprocessedWave(IAsyncEnumerable<byte[]> dataChunks, CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();

        await foreach(var chunk in dataChunks)
        {
            await memoryStream.WriteAsync(chunk, cancellationToken);

            Console.WriteLine($"Received: {chunk.Length} bytes.");
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

public class MyHub : Hub
{
    public async Task TranscribeAudio([FromKeyedServices("wave")] IWaveService waveService, IAsyncEnumerable<byte[]> dataChunks, TrimmedAudioMetadataDTO audioMetadata)
    {
        Console.WriteLine("Connected");

        using var waveStream = await waveService.WriteToWaveStreamAsync(dataChunks, audioMetadata, CancellationToken.None);

        // whisper load
        var modelPath = Path.Combine("WhisperModels", "ggml-small.bin");
        if (!File.Exists(modelPath))
        {
            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Small);
            using var fileWriter = File.OpenWrite(modelPath);
            await modelStream.CopyToAsync(fileWriter);
        }

        using var whisperFactory = WhisperFactory.FromPath(modelPath);

        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("en")
            .Build();

        Console.WriteLine("Whisper loaded");

        var segments = processor.ProcessAsync(waveStream);

        await Clients.Caller.SendAsync("SetStatus", "Transcribing...");

        Console.WriteLine("Starting transcribing...");

        await foreach (var result in segments)
        {
            var subtitle = new Subtitle()
            {
                StartTime = result.Start + audioMetadata.StartTimeOffset,
                Text = result.Text,
                EndTime = result.End + audioMetadata.StartTimeOffset,
            };

            Console.WriteLine($"{subtitle.StartTime}: {subtitle.Text}");

            await Clients.Caller.SendAsync("ShowSubtitle", subtitle);
        }

        await Clients.Caller.SendAsync("SetStatus", "Done.");
    }
}