using System.Text;
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

    public event Action<MarkdownUpdate>? UpdateAvailable;

    public long ProcessedBatchCount { get; private set; }

    public MarkdownStreamScheduler(
        MarkdownStreamProcessor processor)
    {
        ArgumentNullException.ThrowIfNull(processor);

        _processor = processor;
    }

    public MarkdownUpdate Begin()
    {
        lock (_sync)
        {
            if (_workerTask is not null)
            {
                throw new InvalidOperationException(
                    "The Markdown stream is already active.");
            }

            _pendingChunks.Clear();

            _isCompleting = false;
            _isProcessing = false;
            _hasFailed = false;

            _processingCompletion = null;

            ProcessedBatchCount = 0;

            var update =
                _processor.Begin();

            _workerCancellation =
                new CancellationTokenSource();

            var cancellationToken =
                _workerCancellation.Token;

            _workerTask =
                Task.Run(
                    () => ProcessLoopAsync(
                        cancellationToken),
                    cancellationToken);

            return update;
        }
    }

    public void Append(string chunk)
    {
        ArgumentNullException.ThrowIfNull(chunk);

        if (chunk.Length == 0)
        {
            return;
        }

        lock (_sync)
        {
            if (_workerTask is null ||
                _isCompleting ||
                _hasFailed)
            {
                throw new InvalidOperationException(
                    "The Markdown stream is not active.");
            }

            _pendingChunks.Enqueue(chunk);
        }
    }

    public async Task CompleteAsync(
        CancellationToken cancellationToken = default)
    {
        Task? processingTask;

        lock (_sync)
        {
            if (_workerTask is null ||
                _isCompleting ||
                _hasFailed)
            {
                throw new InvalidOperationException(
                    "The Markdown stream is not active.");
            }

            _isCompleting = true;

            processingTask =
                _processingCompletion?.Task;
        }

        // Wait for any currently running processor operation.
        if (processingTask is not null)
        {
            await processingTask.WaitAsync(
                cancellationToken);
        }

        // Process anything that arrived before completion.
        await ProcessPendingAsync(
            cancellationToken);

        // A second check is required because another worker
        // operation could have started while we were processing.
        while (true)
        {
            lock (_sync)
            {
                if (!_isProcessing &&
                    _pendingChunks.Count == 0)
                {
                    break;
                }

                processingTask =
                    _processingCompletion?.Task;
            }

            if (processingTask is not null)
            {
                await processingTask.WaitAsync(
                    cancellationToken);
            }

            await ProcessPendingAsync(
                cancellationToken);
        }

        MarkdownUpdate? update;
        bool failed;

        lock (_sync)
        {
            failed = _hasFailed;

            if (failed)
            {
                update = null;
            }
            else
            {
                update =
                    _processor.Complete();
            }

            _workerCancellation?.Cancel();
        }

        if (failed)
        {
            await StopWorkerAsync();

            throw new InvalidOperationException(
                "The Markdown stream has failed.");
        }

        UpdateAvailable?.Invoke(update!);

        await StopWorkerAsync();
    }

    public void Reset()
    {
        CancellationTokenSource? cancellation;

        lock (_sync)
        {
            cancellation =
                _workerCancellation;

            cancellation?.Cancel();

            _pendingChunks.Clear();

            _isCompleting = false;
            _isProcessing = false;
            _hasFailed = false;

            _processingCompletion = null;

            _processor.Reset();

            _workerTask = null;

            _workerCancellation = null;
        }

        cancellation?.Dispose();
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
        catch (OperationCanceledException)
        {
            // Expected when the stream completes,
            // fails, is cancelled, or resets.
        }
    }

    private async Task ProcessPendingAsync(
        CancellationToken cancellationToken)
    {
        string? combinedChunk = null;

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
            }

            combinedChunk =
                builder.ToString();
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            ProcessedBatchCount++;

            MarkdownUpdate update;

            try
            {
                update =
                    _processor.Append(
                        combinedChunk);
            }
            catch (Exception exception)
            {
                update =
                    _processor.Fail(exception);

                lock (_sync)
                {
                    _hasFailed = true;

                    _isCompleting = true;

                    _pendingChunks.Clear();

                    _workerCancellation?.Cancel();
                }
            }

            UpdateAvailable?.Invoke(update);
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

            completionSource.TrySetResult(true);
        }
    }

    private async Task StopWorkerAsync()
    {
        Task? worker;
        CancellationTokenSource? cancellation;

        lock (_sync)
        {
            worker =
                _workerTask;

            cancellation =
                _workerCancellation;

            _workerTask = null;
            _workerCancellation = null;
        }

        cancellation?.Cancel();

        if (worker is not null)
        {
            try
            {
                await worker;
            }
            catch (OperationCanceledException)
            {
                // Expected during completion/reset/failure.
            }
        }

        cancellation?.Dispose();
    }

    public async Task CancelAsync(
        CancellationToken cancellationToken = default)
    {
        CancellationTokenSource? cancellation;

        lock (_sync)
        {
            if (_workerTask is null ||
                _isCompleting ||
                _hasFailed)
            {
                throw new InvalidOperationException(
                    "The Markdown stream is not active.");
            }

            _isCompleting = true;

            // Anything waiting in the queue should not be
            // processed after cancellation.
            _pendingChunks.Clear();

            cancellation =
                _workerCancellation;
        }

        cancellation?.Cancel();

        Task? processingTask;

        lock (_sync)
        {
            processingTask =
                _processingCompletion?.Task;
        }

        if (processingTask is not null)
        {
            await processingTask.WaitAsync(
                cancellationToken);
        }

        MarkdownUpdate update;

        lock (_sync)
        {
            update =
                _processor.Cancel();
        }

        UpdateAvailable?.Invoke(update);

        await StopWorkerAsync();
    }
}