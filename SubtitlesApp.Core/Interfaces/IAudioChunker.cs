using SubtitlesApp.Core.DTOs;
using SubtitlesApp.Core.Models;
using SubtitlesApp.Core.Result;

namespace SubtitlesApp.Core.Interfaces;

public interface IAudioChunker
{
    IAsyncEnumerable<Result<AudioChunkDto>> ChunkAsync(
        string audioPath,
        TimeInterval timeInterval,
        CancellationToken cancellationToken
    );
}
