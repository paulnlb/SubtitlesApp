using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Application.Services;

public class CancellationManager : ICancellationManager
{
    readonly Dictionary<string, CancellationTokenSource> _ctsDictionary = [];

    readonly object _lock = new();

    public void CancelTask(string key)
    {
        RemoveTaskFromDict(key, shouldCancel: true);
    }

    public CancellationToken RegisterTask(string key)
    {
        var cts = new CancellationTokenSource();
        _ctsDictionary.Add(key, cts);
        return cts.Token;
    }

    public void RemoveTask(string key)
    {
        RemoveTaskFromDict(key);
    }

    private void RemoveTaskFromDict(string key, bool shouldCancel = false)
    {
        lock (_lock)
        {
            if (_ctsDictionary.TryGetValue(key, out var cts))
            {
                if (shouldCancel)
                {
                    cts.Cancel();
                }
                
                cts.Dispose();

                _ctsDictionary.Remove(key);
            }
        }
    }
}
