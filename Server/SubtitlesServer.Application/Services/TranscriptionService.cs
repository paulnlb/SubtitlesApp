using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;
using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Application.Services;

public class TranscriptionService : ITranscriptionService
{
    private readonly IWaveService _waveService;
    private readonly IWhisperService _whisperService;

    public TranscriptionService(IWaveService waveService, IWhisperService whisperService)
    {
        _waveService = waveService;
        _whisperService = whisperService;
    }

    public async IAsyncEnumerable<Subtitle> TranscribeAudioAsync(IAsyncEnumerable<byte[]> dataChunks, TrimmedAudioMetadataDTO audioMetadata)
    {
        using var waveStream = await _waveService.WriteToWaveStreamAsync(dataChunks, audioMetadata, CancellationToken.None);

        Console.WriteLine("Wave read");

        var subtitles = _whisperService.TranscribeAudioAsync(waveStream, audioMetadata.StartTimeOffset);

        await foreach (var subtitle in subtitles)
        {
            yield return subtitle;
        }
    }
}
