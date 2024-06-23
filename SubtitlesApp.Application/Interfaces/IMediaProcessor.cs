using SubtitlesApp.Application.Interfaces.Socket;
using SubtitlesApp.Core.Models;

namespace SubtitlesApp.Application.Interfaces;

/// <summary>
/// Processes media and passes it to a socket or a file
/// </summary>
public interface IMediaProcessor : IDisposable
{
    /// <summary>
    /// Media source path
    /// </summary>
    string SourcePath { get; }

    /// <summary>
    ///     Audio information (format, sample rate etc)
    /// </summary>
    TrimmedAudioMetadata TrimmedAudioMetadata { get; }

    /// <summary>
    /// Exctract audio from source and pass into Socket
    /// </summary>
    /// <param name="startTime">Start time for exctracted audio</param>
    /// <param name="endTime">End time for exctracted audio</param>
    /// <param name="cancellationToken"></param>
    /// <param name="destinationSocket">ISocketSender instance where the processed output is passed</param>
    /// <returns></returns>
    Task ExtractAudioAsync(ISocketSender destinationSocket, CancellationToken cancellationToken);
}
