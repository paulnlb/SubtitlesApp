using SubtitlesServer.Application.Interfaces;

namespace SubtitlesServer.Application.Services;

public class CancellationManager : ICancellationManager
{
    readonly Dictionary<string, CancellationTokenSource> _ctsDictionary = [];

    public void CancelTask(string key)
    {
        if (_ctsDictionary.TryGetValue(key, out var cts))
        {
            cts.Cancel();
            cts.Dispose();

            _ctsDictionary.Remove(key);
        }
    }

    public CancellationToken RegisterTask(string key)
    {
        var cts = new CancellationTokenSource();
        _ctsDictionary.Add(key, cts);
        return cts.Token;
    }
}
