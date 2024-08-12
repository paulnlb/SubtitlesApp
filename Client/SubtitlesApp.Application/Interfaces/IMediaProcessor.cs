using SubtitlesApp.Shared.DTOs;

namespace SubtitlesApp.Application.Interfaces;

/// <summary>
/// Processes media and passes it to a socket or a file
/// </summary>
public interface IMediaProcessor : IDisposable
{
    /// <summary>
    ///     Exctract audio from source
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="startTime"></param>
    /// <param name="duration"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    (TrimmedAudioMetadataDTO Metadata, IAsyncEnumerable<byte[]> AudioBytes) ExtractAudioAsync(
        string sourcePath,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken);
}
