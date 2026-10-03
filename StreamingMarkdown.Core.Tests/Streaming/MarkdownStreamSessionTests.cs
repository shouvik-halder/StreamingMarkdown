
using StreamingMarkdown.Core.Results;
using StreamingMarkdown.Core.Streaming;
using StreamingMarkdown.Maui.Streaming;
using Xunit;

namespace StreamingMarkdown.Core.Tests.Streaming;

public sealed class MarkdownStreamSessionTests
{
    private static MarkdownStreamSession CreateSession()
    {
        return new MarkdownStreamSession(
            MarkdownStreamScheduler.Create());
    }

    [Fact]
    public async Task ConcurrentCompleteAsync_SharesOperation()
    {
        await using var session = CreateSession();

        var completedUpdates = 0;

        session.UpdateAvailable += update =>
        {
            if (update.IsCompleted)
            {
                Interlocked.Increment(ref completedUpdates);
            }
        };

        session.Append("Hello ");
        session.Append("world");

        await Task.WhenAll(
            session.CompleteAsync(),
            session.CompleteAsync());

        Assert.Equal(1, completedUpdates);
    }

    [Fact]
    public async Task CancelledWait_DoesNotCancelCompletion()
    {
        await using var session = CreateSession();

        var completed = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        session.UpdateAvailable += update =>
        {
            if (update.IsCompleted)
            {
                completed.TrySetResult(true);
            }
        };

        session.Append("Hello");

        using var cancellation = new CancellationTokenSource();

        // Start the shared completion operation, then cancel only this wait.
        var firstWait = session.CompleteAsync(cancellation.Token);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await firstWait);

        // A second caller can still await the same completion operation.
        await session.CompleteAsync();

        Assert.True(
            await completed.Task.WaitAsync(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task ConcurrentCancelAsync_SharesOperation()
    {
        await using var session = CreateSession();

        var cancelledUpdates = 0;

        session.UpdateAvailable += update =>
        {
            if (update.Result == MarkdownStreamResult.Cancelled)
            {
                Interlocked.Increment(ref cancelledUpdates);
            }
        };

        session.Append("Some content");

        await Task.WhenAll(
            session.CancelAsync(),
            session.CancelAsync());

        Assert.Equal(1, cancelledUpdates);
    }

    [Fact]
    public async Task DisposeAsync_ActiveSession_PreventsFurtherAppends()
    {
        var session = CreateSession();

        session.Append("Some content");

        await session.DisposeAsync();

        // In DisposeAsync_ActiveSession_PreventsFurtherAppends
        Assert.Throws<ObjectDisposedException>(
            () => session.Append("More content"));

        // Disposal should be idempotent.
        await session.DisposeAsync();
    }

    [Fact]
    public async Task DisposeAsync_AfterCompletion_IsSafe()
    {
        var session = CreateSession();

        session.Append("Finished content");
        await session.CompleteAsync();

        await session.DisposeAsync();
        await session.DisposeAsync();

        // In DisposeAsync_ActiveSession_PreventsFurtherAppends
        Assert.Throws<ObjectDisposedException>(
            () => session.Append("More content"));
    }

    [Fact]
    public async Task UpdateAvailable_SubscriberException_DoesNotBreakSession()
    {
        await using var session = CreateSession();

        var receivedUpdates = 0;

        session.UpdateAvailable += _ =>
            throw new InvalidOperationException("Test subscriber failure");

        session.UpdateAvailable += _ =>
            Interlocked.Increment(ref receivedUpdates);

        session.Append("Hello");
        await session.CompleteAsync();

        Assert.True(receivedUpdates > 0);
    }
}