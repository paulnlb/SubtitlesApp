using System.Collections.Concurrent;

namespace SubtitlesApp.Core.Utils;

public class TaskQueue
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly ConcurrentQueue<Func<CancellationToken, Task>> _queue =
        new ConcurrentQueue<Func<CancellationToken, Task>>();
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public delegate void ExceptionEventHandler(Exception ex);
    public event ExceptionEventHandler? ExceptionThrown;

    public void EnqueueTask(Func<CancellationToken, Task> task)
    {
        _queue.Enqueue(task);
        _ = ProcessQueueAsync();
    }

    public void CancelAllTasks()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    private async Task ProcessQueueAsync()
    {
        if (!_semaphore.Wait(0))
            return;

        try
        {
            while (_queue.TryDequeue(out var task))
            {
                var token = _cancellationTokenSource.Token;

                try
                {
                    await task(token);
                }
                catch (OperationCanceledException)
                {
                    // Do nothing, jump to the bottom if block
                }
                catch (Exception ex)
                {
                    ExceptionThrown?.Invoke(ex);
                }

                if (token.IsCancellationRequested)
                {
                    // All the task in the queue share a singe cancellation token.
                    // If cancellation requested, clear the queue
                    _queue.Clear();
                    break;
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
