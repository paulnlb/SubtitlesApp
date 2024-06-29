namespace SubtitlesServer.Application.Interfaces;

/// <summary>
/// Manages the cancellation of asynchronous operations.
/// </summary>
public interface ICancellationManager
{
    /// <summary>
    /// Cancels the asynchronous operation.
    /// </summary>
    /// <param name="key">Unique task identifier</param>
    void CancelTask(string key);

    /// <summary>
    /// Registers a task in cancellation manager.
    /// </summary>
    /// <param name="key">Unique task identifier</param>
    /// <returns>Cancellation token for the task</returns>
    CancellationToken RegisterTask(string key);

    /// <summary>
    /// Removes a task from the cancellation manager.
    /// </summary>
    /// <param name="key">Unique task identifier</param>
    void RemoveTask(string key);
}
