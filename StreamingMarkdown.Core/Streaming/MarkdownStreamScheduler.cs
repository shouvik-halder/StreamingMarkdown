using System.Text;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Results;

namespace StreamingMarkdown.Core.Streaming;

public sealed class MarkdownStreamScheduler
{
    private readonly MarkdownStreamProcessor _processor;
    private readonly object _sync = new();
    private readonly Queue<string> _pendingChunks = new();

    private CancellationTokenSource? _workerCancellation;
    private Task? _workerTask;
    private TaskCompletionSource<bool>? _processingCompletion;

    private bool _isProcessing;
    private bool _isCompleting;
    private bool _hasFailed;
    private Exception? _failureException;
    private Task? _terminalTask;
    private enum TerminalOperation
{
    None,
    Completing,
    Cancelling
}

private TerminalOperation _terminalOperation;
private bool _isResetting;

    public event Action<MarkdownUpdate>? UpdateAvailable;

    public long ProcessedBatchCount { get; private set; }

    public TimeSpan TotalProcessingTime { get; private set; }

    public long ProcessedChunkCount { get; private set; }

    public MarkdownStreamScheduler(
        MarkdownStreamProcessor processor)
    {
        _processor =
            processor ??
            throw new ArgumentNullException(
                nameof(processor));
    }

    public MarkdownUpdate Begin()
    {
        lock (_sync)
        {
            if (_isResetting)
            {
                throw new InvalidOperationException(
                    "The stream is being reset.");
            }

            if (_workerTask is not null)
            {
                throw new InvalidOperationException(
                    "A stream is already active.");
            }

            if (_terminalTask is { IsCompleted: false })
            {
                throw new InvalidOperationException(
                    "The previous stream operation has not finished.");
            }

            _terminalTask = null;
            _terminalOperation = TerminalOperation.None;

            _pendingChunks.Clear();

            _isProcessing = false;
            _isCompleting = false;
            _hasFailed = false;
            _failureException = null;

            ProcessedBatchCount = 0;
            TotalProcessingTime = TimeSpan.Zero;
            ProcessedChunkCount = 0;

            var update =
                _processor.Begin();

            _workerCancellation =
                new CancellationTokenSource();

            var cancellationToken =
                _workerCancellation.Token;

            _workerTask =
                Task.Run(
                    () => ProcessLoopAsync(cancellationToken),
                    cancellationToken);

            return update;
        }
    }

    public void Append(string chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        lock (_sync)
        {
            if (_workerTask is null)
            {
                throw new InvalidOperationException(
                    "The stream has not been started.");
            }

            if (_isCompleting)
            {
                throw new InvalidOperationException(
                    "The stream is completing.");
            }

            if (_hasFailed)
            {
                throw new InvalidOperationException(
                    "The stream has failed.");
            }
            if (_isResetting)
            {
                throw new InvalidOperationException(
                    "The stream is being reset.");
            }

            _pendingChunks.Enqueue(chunk);
        }
    }

    public async Task CompleteAsync(
    CancellationToken cancellationToken = default)
{
    Task operation;

    lock (_sync)
    {
        if (_isResetting)
        {
            throw new InvalidOperationException(
                "The stream is being reset.");
        }

        if (_terminalOperation == TerminalOperation.Cancelling)
        {
            throw new InvalidOperationException(
                "The stream has been cancelled.");
        }

        if (_terminalTask is null)
        {
            if (_workerTask is null)
            {
                throw new InvalidOperationException(
                    "The stream has not been started.");
            }

            _isCompleting = true;
            _terminalOperation = TerminalOperation.Completing;
            _terminalTask = Task.Run(CompleteCoreAsync);
        }

        operation = _terminalTask;
    }

    await operation.WaitAsync(cancellationToken)
        .ConfigureAwait(false);
}

    public async Task CancelAsync(
    CancellationToken cancellationToken = default)
{
    Task? operation;

    lock (_sync)
    {
        if (_isResetting)
        {
            throw new InvalidOperationException(
                "The stream is being reset.");
        }

        if (_terminalOperation == TerminalOperation.Completing)
        {
            throw new InvalidOperationException(
                "Cannot cancel while completion is in progress.");
        }

        if (_terminalTask is null)
        {
            if (_workerTask is null)
                return;

            _isCompleting = true;
            _terminalOperation = TerminalOperation.Cancelling;
            _terminalTask = Task.Run(CancelCoreAsync);
        }
        else if (_terminalOperation != TerminalOperation.Cancelling)
        {
            throw new InvalidOperationException(
                "The stream is not available for cancellation.");
        }

        operation = _terminalTask;
    }

    if (operation is not null)
    {
        await operation.WaitAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
    public void Reset()
    {
        Task? workerTask;
        Task? processingTask;
        CancellationTokenSource? workerCancellation;

        lock (_sync)
        {
            if (_isResetting)
            {
                throw new InvalidOperationException(
                    "The stream is already being reset.");
            }

            if (_terminalTask is { IsCompleted: false })
            {
                throw new InvalidOperationException(
                    "Cannot reset while completion or cancellation " +
                    "is in progress.");
            }

            _isResetting = true;
            _isCompleting = true;
            _pendingChunks.Clear();

            workerTask = _workerTask;
            processingTask = _processingCompletion?.Task;
            workerCancellation = _workerCancellation;

            workerCancellation?.Cancel();
        }

        try
        {
            processingTask?.GetAwaiter().GetResult();

            if (workerTask is not null)
            {
                try
                {
                    workerTask.GetAwaiter().GetResult();
                }
                catch (OperationCanceledException)
                {
                    // Expected: Reset cancels the worker.
                }
            }

            lock (_sync)
            {
                _processor.Reset();

                _pendingChunks.Clear();

                _isProcessing = false;
                _isCompleting = false;
                _hasFailed = false;
                _failureException = null;

                ProcessedBatchCount = 0;
                TotalProcessingTime = TimeSpan.Zero;
                ProcessedChunkCount = 0;

                _processingCompletion = null;
                _workerTask = null;
                _terminalTask = null;
                _terminalOperation = TerminalOperation.None;

                _workerCancellation = null;
                workerCancellation?.Dispose();
            }
        }
        finally
        {
            lock (_sync)
            {
                _isResetting = false;
            }
        }
    }
    private async Task ProcessLoopAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(
                    TimeSpan.FromMilliseconds(16),
                    cancellationToken);

                await ProcessPendingAsync(
                    cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected when the stream is completed, cancelled, or reset.
        }
        catch (Exception exception)
        {
            // Never let an unexpected worker exception disappear. The terminal
            // operation observes this recorded failure and reports it to callers.
            RecordFailure(exception);
        }
    }

    private void RecordFailure(Exception exception)
    {
        lock (_sync)
        {
            _hasFailed = true;
            _failureException ??= exception;
            _isCompleting = true;
            _pendingChunks.Clear();
        }
    }

    private async Task ProcessPendingAsync(
        CancellationToken cancellationToken)
    {
        string? combinedChunk = null;
        var batchChunkCount = 0;

        TaskCompletionSource<bool>? completionSource = null;

        lock (_sync)
        {
            if (_isProcessing ||
                _pendingChunks.Count == 0)
            {
                return;
            }

            _isProcessing = true;

            completionSource =
                new TaskCompletionSource<bool>(
                    TaskCreationOptions.RunContinuationsAsynchronously);

            _processingCompletion =
                completionSource;

            var builder =
                new StringBuilder();

            while (_pendingChunks.Count > 0)
            {
                builder.Append(
                    _pendingChunks.Dequeue());

                batchChunkCount++;
            }

            combinedChunk =
                builder.ToString();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_sync)
            {
                ProcessedBatchCount++;
            }

            MarkdownUpdate update;

            try
            {
                var processingStopwatch =
                    System.Diagnostics.Stopwatch.StartNew();

                update =
                    _processor.Append(
                        combinedChunk);

                processingStopwatch.Stop();

                lock (_sync)
                {
                    TotalProcessingTime +=
                        processingStopwatch.Elapsed;

                    ProcessedChunkCount +=
                        batchChunkCount;
                }
            }
            catch (Exception exception)
            {
                lock (_sync)
                {
                    _hasFailed = true;
                    _failureException ??= exception;
                    _isCompleting = true;
                    _pendingChunks.Clear();
                    _workerCancellation?.Cancel();
                }

                try
                {
                    update = _processor.Fail(exception);
                }
                catch (Exception failureException)
                {
                    lock (_sync)
                    {
                        _failureException = new AggregateException(
                            "Chunk processing and error recovery both failed.",
                            exception,
                            failureException);
                    }

                    throw;
                }
            }

            PublishUpdate(update);
        }
        finally
        {
            lock (_sync)
            {
                _isProcessing = false;

                if (ReferenceEquals(
                    _processingCompletion,
                    completionSource))
                {
                    _processingCompletion = null;
                }
            }

            completionSource.TrySetResult(
                true);
        }
    }

    private async Task StopWorkerAsync()
    {
        Task? workerTask;

        lock (_sync)
        {
            workerTask =
                _workerTask;
        }

        Exception? workerFailure = null;

        try
        {
            if (workerTask is not null)
            {
                try
                {
                    await workerTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Expected during shutdown.
                }
                catch (Exception exception)
                {
                    workerFailure = exception;
                    RecordFailure(exception);
                }
            }
        }
        finally
        {
            lock (_sync)
            {
                if (ReferenceEquals(_workerTask, workerTask))
                {
                    _workerTask = null;
                    _workerCancellation?.Dispose();
                    _workerCancellation = null;
                }
            }
        }

        if (workerFailure is not null)
        {
            throw new InvalidOperationException(
                "The markdown processing worker failed.",
                workerFailure);
        }
    }

    private async Task CompleteCoreAsync()
    {
        try
        {
            while (true)
            {
                Task? processingTask;

                lock (_sync)
                {
                    if (_hasFailed)
                        break;

                    processingTask =
                        _processingCompletion?.Task;
                }

                if (processingTask is not null)
                {
                    await processingTask.ConfigureAwait(false);
                }

                await ProcessPendingAsync(
                    CancellationToken.None).ConfigureAwait(false);

                lock (_sync)
                {
                    if (_hasFailed)
                        break;

                    if (!_isProcessing &&
                        _pendingChunks.Count == 0)
                    {
                        break;
                    }
                }
            }

            Exception? processingFailure;

            lock (_sync)
            {
                processingFailure = _failureException;
            }

            if (processingFailure is not null)
            {
                throw new InvalidOperationException(
                    "The stream failed during processing.",
                    processingFailure);
            }

            MarkdownUpdate update;

            try
            {
                update = _processor.Complete();
            }
            catch (Exception exception)
            {
                RecordFailure(exception);

                try
                {
                    update = _processor.Fail(exception);
                    PublishUpdate(update);
                }
                catch (Exception failureException)
                {
                    throw new InvalidOperationException(
                        "Stream completion failed, and the processor could not create an error update.",
                        new AggregateException(exception, failureException));
                }

                throw new InvalidOperationException(
                    "The stream failed while completing.",
                    exception);
            }

            PublishUpdate(update);
        }
        finally
        {
            _workerCancellation?.Cancel();

            await StopWorkerAsync().ConfigureAwait(false);
        }
    }

    private async Task CancelCoreAsync()
    {
        try
        {
            Task? processingTask;

            lock (_sync)
            {
                _isCompleting = true;
                _pendingChunks.Clear();

                _workerCancellation?.Cancel();

                processingTask =
                    _processingCompletion?.Task;
            }

            if (processingTask is not null)
            {
                await processingTask.ConfigureAwait(false);
            }

            bool hasFailed;

            lock (_sync)
            {
                hasFailed = _hasFailed;
            }

            if (!hasFailed)
            {
                var update = _processor.Cancel();
                PublishUpdate(update);
            }
        }
        finally
        {
            _workerCancellation?.Cancel();

            await StopWorkerAsync().ConfigureAwait(false);
        }
    }

    private void PublishUpdate(MarkdownUpdate update)
    {
        var handlers = UpdateAvailable;

        if (handlers is null)
            return;

        foreach (Action<MarkdownUpdate> handler
            in handlers.GetInvocationList())
        {
            try
            {
                handler(update);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Markdown update subscriber failed: {exception}");
            }
        }
    }
    public static MarkdownStreamScheduler Create()
    {
        var buffer = new MarkdownBuffer();
        var parser = new MarkdigMarkdownParser();
        var reconciler = new DocumentReconciler();
        var diffEngine = new DocumentDiffEngine();

        var processor = new MarkdownStreamProcessor(
            buffer,
            parser,
            reconciler,
            diffEngine);

        return new MarkdownStreamScheduler(processor);
    }
}