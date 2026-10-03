using System.Collections.Concurrent;
using StreamingMarkdown.Core.Results;
using StreamingMarkdown.Core.Streaming;
using StreamingMarkdown.Maui.Streaming;
using Xunit;

namespace StreamingMarkdown.Core.Tests.Streaming;

public sealed class MarkdownStreamSessionStressTests
{
    private static MarkdownStreamSession CreateSession() =>
        new(MarkdownStreamScheduler.Create());

    [Fact]
    public async Task ConcurrentAppendAndComplete_DoesNotProduceUnexpectedExceptions()
    {
        await using var session = CreateSession();
        using var start = new ManualResetEventSlim(false);
        var unexpectedExceptions = new ConcurrentQueue<Exception>();

        var producers = Enumerable.Range(0, 4)
            .Select(worker => Task.Run(() =>
            {
                start.Wait();

                for (var i = 0; i < 250; i++)
                {
                    try
                    {
                        session.Append($"worker {worker}, chunk {i}\n");
                    }
                    catch (InvalidOperationException)
                    {
                        // Expected when CompleteAsync wins the lifecycle race.
                        break;
                    }
                    catch (Exception exception)
                    {
                        unexpectedExceptions.Enqueue(exception);
                        break;
                    }
                }
            }))
            .ToArray();

        var completion = Task.Run(async () =>
        {
            start.Wait();
            await Task.Delay(1);
            await session.CompleteAsync();
        });

        start.Set();
        await Task.WhenAll(producers.Append(completion)).WaitAsync(TimeSpan.FromSeconds(15));

        Assert.Empty(unexpectedExceptions);
    }

    [Fact]
    public async Task CancelAfterFirstUpdate_CompletesAndPublishesCancelledResult()
    {
        await using var session = CreateSession();
        var firstUpdate = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var cancelledUpdates = 0;

        session.UpdateAvailable += update =>
        {
            firstUpdate.TrySetResult(true);

            if (update.Result == MarkdownStreamResult.Cancelled)
            {
                Interlocked.Increment(ref cancelledUpdates);
            }
        };

        session.Append("Initial content\n");
        await firstUpdate.Task.WaitAsync(TimeSpan.FromSeconds(5));

        // Queue enough data to overlap cancellation with scheduler activity.
        for (var i = 0; i < 2_000; i++)
        {
            session.Append($"Additional content {i}\n");
        }

        await session.CancelAsync().WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Equal(1, Volatile.Read(ref cancelledUpdates));
        Assert.Throws<InvalidOperationException>(() => session.Append("after cancel"));
    }

    [Fact]
    public async Task ConcurrentDisposeCalls_AreIdempotent()
    {
        var session = CreateSession();

        var disposals = Enumerable.Range(0, 20)
            .Select(_ => Task.Run(async () => await session.DisposeAsync().AsTask()))
            .ToArray();

        await Task.WhenAll(disposals).WaitAsync(TimeSpan.FromSeconds(10));

        Assert.Throws<ObjectDisposedException>(() => session.Append("after dispose"));
    }
}
