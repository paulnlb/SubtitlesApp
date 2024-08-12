using SubtitlesApp.Core.Models;
using SubtitlesApp.Shared.DTOs;
using SubtitlesServer.Application.Interfaces;
using System.Runtime.CompilerServices;

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

    public async IAsyncEnumerable<Subtitle> TranscribeAudioAsync(
        IAsyncEnumerable<byte[]> dataChunks, 
        TrimmedAudioMetadataDTO audioMetadata,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var waveStream = await _waveService.WriteToWaveStreamAsync(dataChunks, audioMetadata, cancellationToken);

        var subtitles = _whisperService.TranscribeAudioAsync(waveStream, audioMetadata.StartTimeOffset, cancellationToken);

        await foreach (var subtitle in subtitles)
        {
            yield return subtitle;
        }
    }
}
