using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.DTOs;
using SubtitlesServer.Application.Configs;
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
        SpeechToTextConfigs speechToTextConfigs,
        WhisperConfigs whisperConfigs,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var waveStream = await _waveService.WriteToWaveStreamAsync(dataChunks, audioMetadata, cancellationToken);

        var subtitles = _whisperService.TranscribeAudioAsync(
            waveStream,
            speechToTextConfigs,
            whisperConfigs,
            cancellationToken);

        await foreach (var subtitle in subtitles)
        {
            yield return subtitle;
        }
    }
}
