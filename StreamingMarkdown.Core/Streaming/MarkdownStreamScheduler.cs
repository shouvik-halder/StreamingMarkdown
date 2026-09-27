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
            if (_workerTask is not null)
            {
                throw new InvalidOperationException(
                    "A stream is already active.");
            }

            _pendingChunks.Clear();

            _isProcessing = false;
            _isCompleting = false;
            _hasFailed = false;

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

            _pendingChunks.Enqueue(chunk);
        }
    }

    public async Task CompleteAsync(
        CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            if (_workerTask is null)
            {
                throw new InvalidOperationException(
                    "The stream has not been started.");
            }

            if (_hasFailed)
            {
                throw new InvalidOperationException(
                    "The stream has already failed.");
            }

            _isCompleting = true;
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            await ProcessPendingAsync(
                cancellationToken);

            lock (_sync)
            {
                if (!_isProcessing &&
                    _pendingChunks.Count == 0)
                {
                    break;
                }
            }
        }

        MarkdownUpdate completedUpdate;

        try
        {
            completedUpdate =
                _processor.Complete();
        }
        catch (Exception exception)
        {
            completedUpdate =
                _processor.Fail(exception);

            lock (_sync)
            {
                _hasFailed = true;
            }
        }

        UpdateAvailable?.Invoke(
            completedUpdate);

        _workerCancellation?.Cancel();

        await StopWorkerAsync();
    }

    public async Task CancelAsync(
        CancellationToken cancellationToken = default)
    {
        Task? processingTask;

        lock (_sync)
        {
            if (_workerTask is null)
            {
                return;
            }

            _isCompleting = true;

            _pendingChunks.Clear();

            _workerCancellation?.Cancel();

            processingTask =
                _processingCompletion?.Task;
        }

        if (processingTask is not null)
        {
            await processingTask.WaitAsync(
                cancellationToken);
        }

        var cancelledUpdate =
            _processor.Cancel();

        UpdateAvailable?.Invoke(
            cancelledUpdate);

        await StopWorkerAsync();
    }

    public void Reset()
    {
        lock (_sync)
        {
            _workerCancellation?.Cancel();

            _pendingChunks.Clear();

            _isProcessing = false;
            _isCompleting = false;
            _hasFailed = false;

            ProcessedBatchCount = 0;
            TotalProcessingTime = TimeSpan.Zero;
            ProcessedChunkCount = 0;

            _processingCompletion = null;
            _workerTask = null;

            _workerCancellation?.Dispose();
            _workerCancellation = null;

            _processor.Reset();
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
        catch (OperationCanceledException)
        {
            // Expected when the stream is completed,
            // cancelled, or reset.
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

            ProcessedBatchCount++;

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
                update =
                    _processor.Fail(
                        exception);

                lock (_sync)
                {
                    _hasFailed = true;
                    _isCompleting = true;

                    _pendingChunks.Clear();

                    _workerCancellation?.Cancel();
                }
            }

            UpdateAvailable?.Invoke(
                update);
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

        if (workerTask is not null)
        {
            try
            {
                await workerTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during shutdown.
            }
        }

        lock (_sync)
        {
            if (ReferenceEquals(
                _workerTask,
                workerTask))
            {
                _workerTask = null;

                _workerCancellation?.Dispose();

                _workerCancellation = null;
            }
        }
    }
}