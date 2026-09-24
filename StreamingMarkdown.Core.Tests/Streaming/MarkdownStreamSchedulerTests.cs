using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Results;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Core.Tests.Streaming;

public sealed class MarkdownStreamSchedulerTests
{
    [Fact]
    public void Begin_StartsNewStream()
    {
        var processor = CreateProcessor();
        var scheduler = new MarkdownStreamScheduler(processor);

        var update = scheduler.Begin();

        Assert.NotNull(update);
        Assert.False(update.IsCompleted);
        Assert.Equal(1, update.Version);

        scheduler.Reset();
    }

    [Fact]
    public async Task Append_MultipleChunks_AreCoalesced()
    {
        var parser = new RecordingMarkdownParser();
        var processor = CreateProcessor(parser);
        var scheduler = new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        scheduler.Append("Hello ");
        scheduler.Append("world");
        scheduler.Append("!");

        await Task.Delay(50);

        Assert.Contains(
            "Hello world!",
            parser.ParsedContents);

        scheduler.Reset();
    }

    [Fact]
    public async Task Append_RaisesUpdateAvailable()
    {
        var processor = CreateProcessor();
        var scheduler = new MarkdownStreamScheduler(processor);

        MarkdownUpdate? receivedUpdate = null;

        scheduler.UpdateAvailable += update =>
        {
            receivedUpdate = update;
        };

        scheduler.Begin();

        scheduler.Append("Hello");

        await Task.Delay(50);

        Assert.NotNull(receivedUpdate);
        Assert.False(receivedUpdate!.IsCompleted);

        scheduler.Reset();
    }

    [Fact]
    public async Task CompleteAsync_FlushesPendingChunks()
    {
        var parser = new RecordingMarkdownParser();
        var processor = CreateProcessor(parser);
        var scheduler = new MarkdownStreamScheduler(processor);

        MarkdownUpdate? completedUpdate = null;

        scheduler.UpdateAvailable += update =>
        {
            completedUpdate = update;
        };

        scheduler.Begin();

        scheduler.Append("Hello ");
        scheduler.Append("world");

        await scheduler.CompleteAsync();

        Assert.Contains(
            "Hello world",
            parser.ParsedContents);

        Assert.NotNull(completedUpdate);
        Assert.True(completedUpdate!.IsCompleted);
    }

    [Fact]
    public void EmptyChunk_DoesNotThrow()
    {
        var processor = CreateProcessor();
        var scheduler = new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        var exception = Record.Exception(() =>
        {
            scheduler.Append(string.Empty);
        });

        Assert.Null(exception);

        scheduler.Reset();
    }

    [Fact]
    public void Append_BeforeBegin_Throws()
    {
        var processor = CreateProcessor();
        var scheduler = new MarkdownStreamScheduler(processor);

        Assert.Throws<InvalidOperationException>(() =>
        {
            scheduler.Append("Hello");
        });
    }

    [Fact]
    public void Begin_CalledTwice_Throws()
    {
        var processor = CreateProcessor();
        var scheduler = new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        Assert.Throws<InvalidOperationException>(() =>
        {
            scheduler.Begin();
        });

        scheduler.Reset();
    }

    [Fact]
    public async Task Reset_AllowsNewStream()
    {
        var processor = CreateProcessor();
        var scheduler = new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        scheduler.Append("First stream");

        await Task.Delay(50);

        scheduler.Reset();

        var exception = Record.Exception(() =>
        {
            scheduler.Begin();
        });

        Assert.Null(exception);

        scheduler.Reset();
    }

    [Fact]
    public async Task CompleteAsync_WaitsForProcessingToFinish()
    {
        var parser = new BlockingMarkdownParser();

        var processor = CreateProcessor(parser);

        var scheduler =
            new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        scheduler.Append("Hello");

        // Give the scheduler time to start processing.
        await parser.ProcessingStarted.Task;

        var completeTask =
            scheduler.CompleteAsync();

        // CompleteAsync must still be waiting because
        // the processor is intentionally blocked.
        await Task.Delay(25);

        Assert.False(
            completeTask.IsCompleted);

        // Allow the processor to finish.
        parser.ReleaseProcessing();

        await completeTask;

        Assert.True(
            parser.CompletedAfterProcessing);

        scheduler.Reset();
    }

    [Fact]
    public async Task Append_ChunksAreNotLostDuringProcessing()
    {
        var parser = new RecordingMarkdownParser();

        var processor =
            CreateProcessor(parser);

        var scheduler =
            new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        scheduler.Append("Hello ");
        scheduler.Append("world ");
        scheduler.Append("from ");
        scheduler.Append("StreamingMarkdown");

        await scheduler.CompleteAsync();

        Assert.Contains(
            "Hello world from StreamingMarkdown",
            parser.ParsedContents);

        scheduler.Reset();
    }

    [Fact]
    public async Task CompleteAsync_EmitsCompletedUpdateAfterFinalProcessing()
    {
        var parser =
            new RecordingMarkdownParser();

        var processor =
            CreateProcessor(parser);

        var scheduler =
            new MarkdownStreamScheduler(processor);

        var updates =
            new List<MarkdownUpdate>();

        scheduler.UpdateAvailable += update =>
        {
            updates.Add(update);
        };

        scheduler.Begin();

        scheduler.Append("Hello");
        scheduler.Append(" world");

        await scheduler.CompleteAsync();

        Assert.NotEmpty(updates);

        Assert.True(
            updates[^1].IsCompleted);

        Assert.All(
            updates.Take(updates.Count - 1),
            update => Assert.False(update.IsCompleted));

        scheduler.Reset();
    }

    [Fact]
    public async Task ConcurrentAppends_DoNotLoseChunks()
    {
        var parser = new RecordingMarkdownParser();

        var processor =
            CreateProcessor(parser);

        var scheduler =
            new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        var tasks = Enumerable
            .Range(0, 100)
            .Select(i =>
                Task.Run(() =>
                {
                    scheduler.Append($"chunk-{i} ");
                }))
            .ToArray();

        await Task.WhenAll(tasks);

        await scheduler.CompleteAsync();

        var parsedContent =
            string.Join(
                " ",
                parser.ParsedContents);

        for (var i = 0; i < 100; i++)
        {
            Assert.Contains(
                $"chunk-{i}",
                parsedContent);
        }

        scheduler.Reset();
    }

    [Fact]
    public async Task CancelAsync_EmitsCancelledUpdate()
    {
        var processor =
            CreateProcessor();

        var scheduler =
            new MarkdownStreamScheduler(processor);

        MarkdownUpdate? cancelledUpdate = null;

        scheduler.UpdateAvailable += update =>
        {
            if (update.Result ==
                MarkdownStreamResult.Cancelled)
            {
                cancelledUpdate = update;
            }
        };

        scheduler.Begin();

        scheduler.Append("Hello");

        await scheduler.CancelAsync();

        Assert.NotNull(cancelledUpdate);

        Assert.Equal(
            MarkdownStreamResult.Cancelled,
            cancelledUpdate!.Result);

        Assert.False(
            cancelledUpdate.IsCompleted);
    }


    [Fact]
    public async Task CancelAsync_DiscardsPendingChunks()
    {
        var parser =
            new RecordingMarkdownParser();

        var processor =
            CreateProcessor(parser);

        var scheduler =
            new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        scheduler.Append("Hello ");
        scheduler.Append("world ");
        scheduler.Append("this ");
        scheduler.Append("should ");
        scheduler.Append("not process");

        await scheduler.CancelAsync();

        Assert.DoesNotContain(
            "should not process",
            parser.ParsedContents);
    }

    [Fact]
    public async Task Append_AfterCancellation_Throws()
    {
        var processor =
            CreateProcessor();

        var scheduler =
            new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        await scheduler.CancelAsync();

        Assert.Throws<InvalidOperationException>(() =>
        {
            scheduler.Append("This should fail");
        });
    }

    [Fact]
    public async Task Complete_AfterCancellation_Throws()
    {
        var processor =
            CreateProcessor();

        var scheduler =
            new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        await scheduler.CancelAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                await scheduler.CompleteAsync();
            });
    }

    [Fact]
    public async Task CancelAsync_AllowsNewStream()
    {
        var processor =
            CreateProcessor();

        var scheduler =
            new MarkdownStreamScheduler(processor);

        scheduler.Begin();

        await scheduler.CancelAsync();

        var exception =
            Record.Exception(() =>
            {
                scheduler.Begin();
            });

        Assert.Null(exception);

        scheduler.Reset();
    }

[Fact]
public async Task ProcessingFailure_EmitsFailedUpdate()
{
    var exception =
        new InvalidOperationException("Parser failed.");

    var parser =
        new ThrowingMarkdownParser(exception);

    var processor =
        CreateProcessor(parser);

    var scheduler =
        new MarkdownStreamScheduler(processor);

    var failedUpdate =
        new TaskCompletionSource<MarkdownUpdate>(
            TaskCreationOptions.RunContinuationsAsynchronously);

    scheduler.UpdateAvailable += update =>
    {
        if (update.Result ==
            MarkdownStreamResult.Failed)
        {
            failedUpdate.TrySetResult(update);
        }
    };

    scheduler.Begin();

    scheduler.Append("Hello");

    var completedTask =
        await Task.WhenAny(
            failedUpdate.Task,
            Task.Delay(TimeSpan.FromSeconds(2)));

    Assert.Same(
        failedUpdate.Task,
        completedTask);

    var update =
        await failedUpdate.Task;

    Assert.Equal(
        MarkdownStreamResult.Failed,
        update.Result);

    Assert.True(
        update.HasError);

    Assert.Equal(
        "Parser failed.",
        update.ErrorMessage);

    Assert.Empty(
        update.Document.Blocks);

    scheduler.Reset();
}
    [Fact]
public async Task ProcessingFailure_EmitsFailedUpdateDuringProcessing()
{
    var exception =
        new InvalidOperationException("Processing failed.");

    var parser =
        new ThrowingMarkdownParser(exception);

    var processor =
        CreateProcessor(parser);

    var scheduler =
        new MarkdownStreamScheduler(processor);

    var updates =
        new List<MarkdownUpdate>();

    scheduler.UpdateAvailable += update =>
    {
        updates.Add(update);
    };

    scheduler.Begin();

    scheduler.Append("Hello");

    await Task.Delay(100);

    Assert.Contains(
        updates,
        update =>
            update.Result ==
            MarkdownStreamResult.Failed);

    var failedUpdate =
        updates.Single(
            update =>
                update.Result ==
                MarkdownStreamResult.Failed);

    Assert.Equal(
        "Processing failed.",
        failedUpdate.ErrorMessage);
}

[Fact]
public async Task ProcessingFailure_StopsFurtherProcessing()
{
    var exception =
        new InvalidOperationException("Processing failed.");

    var parser =
        new ThrowingMarkdownParser(exception);

    var processor =
        CreateProcessor(parser);

    var scheduler =
        new MarkdownStreamScheduler(processor);

    var updates =
        new List<MarkdownUpdate>();

    scheduler.UpdateAvailable += update =>
    {
        updates.Add(update);
    };

    scheduler.Begin();

    scheduler.Append("First");

    await Task.Delay(100);

    Assert.Contains(
        updates,
        update =>
            update.Result ==
            MarkdownStreamResult.Failed);

    Assert.Throws<InvalidOperationException>(
        () => scheduler.Append("Second"));
}

[Fact]
public async Task Reset_AfterProcessingFailure_AllowsNewStream()
{
    var exception =
        new InvalidOperationException("Parser failed.");

    var parser =
        new ThrowingMarkdownParser(exception);

    var processor =
        CreateProcessor(parser);

    var scheduler =
        new MarkdownStreamScheduler(processor);

    var failedUpdate =
        new TaskCompletionSource<MarkdownUpdate>(
            TaskCreationOptions.RunContinuationsAsynchronously);

    scheduler.UpdateAvailable += update =>
    {
        if (update.Result ==
            MarkdownStreamResult.Failed)
        {
            failedUpdate.TrySetResult(update);
        }
    };

    scheduler.Begin();

    scheduler.Append("This will fail.");

    await Task.WhenAny(
        failedUpdate.Task,
        Task.Delay(TimeSpan.FromSeconds(2)));

    Assert.True(
        failedUpdate.Task.IsCompleted);

    scheduler.Reset();

    var newStream =
        scheduler.Begin();

    Assert.Equal(
        MarkdownStreamResult.Streaming,
        newStream.Result);

    scheduler.Reset();
}

[Fact]
public async Task Processing_RunsOnBackgroundThread()
{
    var parser =
        new ThreadRecordingMarkdownParser();

    var processor =
        CreateProcessor(parser);

    var scheduler =
        new MarkdownStreamScheduler(processor);

    var callerThreadId =
        Environment.CurrentManagedThreadId;

    var processingCompleted =
        new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

    scheduler.UpdateAvailable += update =>
    {
        if (update.Result ==
            MarkdownStreamResult.Streaming)
        {
            processingCompleted.TrySetResult(true);
        }
    };

    scheduler.Begin();

    scheduler.Append("Hello");

    var completedTask =
        await Task.WhenAny(
            processingCompleted.Task,
            Task.Delay(TimeSpan.FromSeconds(2)));

    Assert.Same(
        processingCompleted.Task,
        completedTask);

    Assert.NotEqual(
        callerThreadId,
        parser.ParseThreadId);

    scheduler.Reset();
}


[Fact]
public async Task Append_DoesNotBlockWhileProcessing()
{
    var parser =
        new BlockingMarkdownParser();

    var processor =
        CreateProcessor(parser);

    var scheduler =
        new MarkdownStreamScheduler(processor);

    scheduler.Begin();

    var stopwatch =
        System.Diagnostics.Stopwatch.StartNew();

    scheduler.Append("Hello");

    stopwatch.Stop();

    Assert.True(
        stopwatch.ElapsedMilliseconds < 50);

    await parser.ProcessingStarted.Task;

    parser.ReleaseProcessing();

    scheduler.Reset();
}

    private static MarkdownStreamProcessor CreateProcessor(
        IMarkdownParser? parser = null)
    {
        parser ??= new RecordingMarkdownParser();

        return new MarkdownStreamProcessor(
            new MarkdownBuffer(),
            parser,
            new DocumentReconciler(),
            new DocumentDiffEngine());
    }

    private sealed class ThreadRecordingMarkdownParser : IMarkdownParser
{
    public int ParseThreadId { get; private set; }

    public MarkdownDocument Parse(string markdown)
    {
        ParseThreadId =
            Environment.CurrentManagedThreadId;

        return MarkdownDocument.Empty;
    }
}

    private sealed class RecordingMarkdownParser : IMarkdownParser
    {
        public List<string> ParsedContents { get; } = [];

        public MarkdownDocument Parse(string markdown)
        {
            ParsedContents.Add(markdown);

            return MarkdownDocument.Empty;
        }
    }

    private sealed class BlockingMarkdownParser : IMarkdownParser
    {
        public TaskCompletionSource<bool> ProcessingStarted { get; } =
            new(
                TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly TaskCompletionSource<bool> _releaseProcessing =
            new(
                TaskCreationOptions.RunContinuationsAsynchronously);

        public bool CompletedAfterProcessing { get; private set; }

        public MarkdownDocument Parse(string markdown)
        {
            ProcessingStarted.TrySetResult(true);

            _releaseProcessing.Task
                .GetAwaiter()
                .GetResult();

            CompletedAfterProcessing = true;

            return MarkdownDocument.Empty;
        }

        public void ReleaseProcessing()
        {
            _releaseProcessing.TrySetResult(true);
        }
    }

    private sealed class ThrowingMarkdownParser : IMarkdownParser
    {
        private readonly Exception _exception;

        public ThrowingMarkdownParser(Exception exception)
        {
            _exception = exception;
        }

        public MarkdownDocument Parse(string markdown)
        {
            throw _exception;
        }
    }
}