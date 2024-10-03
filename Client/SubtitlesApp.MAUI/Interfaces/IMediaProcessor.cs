namespace SubtitlesApp.Maui.Interfaces;

/// <summary>
/// Processes media and returns audio bytes
/// </summary>
public interface IMediaProcessor : IDisposable
{
    /// <summary>
    ///     Exctract audio from source
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<byte[]> ExtractAudioAsync(
        string sourcePath,
        TimeSpan startTime,
        TimeSpan endTime,
        CancellationToken cancellationToken);
}
