using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Results;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Maui.Streaming;

public sealed class MarkdownStreamSession : IMarkdownStreamSession
{
    private enum SessionState
    {
        Active,
        Completing,
        Cancelling,
        Terminal,
        Disposing,
        Disposed
    }

    private readonly object _sync = new();
    private readonly MarkdownStreamScheduler _scheduler;

    private SessionState _state = SessionState.Active;
    private Task? _completionTask;
    private Task? _cancellationTask;
    private Task? _disposalTask;
    private MarkdownUpdate? _latestUpdate;

    public event Action<MarkdownUpdate>? UpdateAvailable;
    public MarkdownUpdate? LatestUpdate
{
    get
    {
        lock (_sync)
        {
            return _latestUpdate;
        }
    }
}

    internal MarkdownStreamSession(MarkdownStreamScheduler scheduler)
    {
        ArgumentNullException.ThrowIfNull(scheduler);

        _scheduler = scheduler;

        try
        {
            _latestUpdate = _scheduler.Begin();
            _scheduler.UpdateAvailable += OnSchedulerUpdate;
        }
        catch
        {
            _scheduler.UpdateAvailable -= OnSchedulerUpdate;
            throw;
        }
    }

    public void Append(string chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        lock (_sync)
        {
            switch (_state)
            {
                case SessionState.Active:
                    _scheduler.Append(chunk);
                    return;

                case SessionState.Disposing:
                case SessionState.Disposed:
                    throw new ObjectDisposedException(nameof(MarkdownStreamSession));

                default:
                    throw new InvalidOperationException(
                        $"Cannot append to a session in state '{_state}'.");
            }
        }
    }

    public Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        Task operation;

        lock (_sync)
        {
            switch (_state)
            {
                case SessionState.Active:
                    _state = SessionState.Completing;
                    _completionTask = Task.Run(CompleteCoreAsync);
                    operation = _completionTask;
                    break;

                case SessionState.Completing:
                    operation = _completionTask!;
                    break;

                case SessionState.Terminal:
                    if (_completionTask is not null)
                    {
                        operation = _completionTask;
                        break;
                    }

                    throw new InvalidOperationException(
                        "The session was cancelled and cannot be completed.");

                case SessionState.Cancelling:
                    throw new InvalidOperationException(
                        "The session is being cancelled.");

                default:
                    throw new ObjectDisposedException(
                        nameof(MarkdownStreamSession),
                        "The session is being disposed or has been disposed.");
            }
        }

        // Caller cancellation stops only this wait, not the shared operation.
        return operation.WaitAsync(cancellationToken);
    }

    public Task CancelAsync(CancellationToken cancellationToken = default)
    {
        Task operation;

        lock (_sync)
        {
            switch (_state)
            {
                case SessionState.Active:
                    _state = SessionState.Cancelling;
                    _cancellationTask = Task.Run(CancelCoreAsync);
                    operation = _cancellationTask;
                    break;

                case SessionState.Cancelling:
                    operation = _cancellationTask!;
                    break;

                case SessionState.Terminal:
                    if (_cancellationTask is not null)
                    {
                        operation = _cancellationTask;
                        break;
                    }

                    throw new InvalidOperationException(
                        "The session has already completed.");

                case SessionState.Completing:
                    throw new InvalidOperationException(
                        "Cannot cancel while completion is in progress.");

                case SessionState.Disposing:
                    operation = _disposalTask!;
                    break;

                default:
                    return Task.CompletedTask;
            }
        }

        // Caller cancellation stops only this wait, not stream cleanup.
        return operation.WaitAsync(cancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        lock (_sync)
        {
            if (_disposalTask is not null)
            {
                return new ValueTask(_disposalTask);
            }

            Task? pendingOperation = null;

            switch (_state)
            {
                case SessionState.Active:
                    _state = SessionState.Disposing;
                    _cancellationTask = Task.Run(CancelCoreAsync);
                    pendingOperation = _cancellationTask;
                    break;

                case SessionState.Completing:
                    pendingOperation = _completionTask;
                    _state = SessionState.Disposing;
                    break;

                case SessionState.Cancelling:
                    pendingOperation = _cancellationTask;
                    _state = SessionState.Disposing;
                    break;

                case SessionState.Terminal:
                    _state = SessionState.Disposing;
                    pendingOperation = _completionTask ?? _cancellationTask;
                    break;

                case SessionState.Disposing:
                    // The existing disposal task is returned above.
                    break;

                case SessionState.Disposed:
                    return ValueTask.CompletedTask;
            }

            _disposalTask = Task.Run(() => DisposeCoreAsync(pendingOperation));
            return new ValueTask(_disposalTask);
        }
    }

    private async Task CompleteCoreAsync()
    {
        try
        {
            await _scheduler.CompleteAsync(CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            lock (_sync)
            {
                if (_state == SessionState.Completing)
                {
                    _state = SessionState.Terminal;
                }
            }
        }
    }

    private async Task CancelCoreAsync()
    {
        try
        {
            await _scheduler.CancelAsync(CancellationToken.None).ConfigureAwait(false);
        }
        finally
        {
            lock (_sync)
            {
                if (_state == SessionState.Cancelling)
                {
                    _state = SessionState.Terminal;
                }
            }
        }
    }

    private async Task DisposeCoreAsync(Task? pendingOperation)
    {
        try
        {
            if (pendingOperation is not null)
            {
                await pendingOperation.ConfigureAwait(false);
            }
        }
        finally
        {
            _scheduler.UpdateAvailable -= OnSchedulerUpdate;

            lock (_sync)
            {
                _state = SessionState.Disposed;
                UpdateAvailable = null;
            }
        }
    }

private void OnSchedulerUpdate(MarkdownUpdate update)
{
    Action<MarkdownUpdate>? handlers;

    lock (_sync)
    {
        if (_state is SessionState.Disposing or SessionState.Disposed)
        {
            return;
        }

        _latestUpdate = update;

        handlers = UpdateAvailable;
    }

    if (handlers is null)
    {
        return;
    }

    foreach (Action<MarkdownUpdate> handler
        in handlers.GetInvocationList())
    {
        try
        {
            handler(update);
        }
        catch (Exception exception)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(
                $"Markdown update subscriber failed: {exception}");
#endif
        }
    }
}
}
