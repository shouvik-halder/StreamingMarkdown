using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks;

public static class CoalescingDiagnostics
{
    public static async Task RunAsync()
    {
        var chunkCounts = new[] { 100, 500 };

        var intervals = new[] { 1, 5, 10, 20 };

        Console.WriteLine(
            "Chunks | Interval | Batches | Chunks/Batch");

        Console.WriteLine(
            "-------|----------|---------|-------------");

        foreach (var chunkCount in chunkCounts)
        {
            foreach (var interval in intervals)
            {
                var processor =
                    CreateProcessor();

                var scheduler =
                    new MarkdownStreamScheduler(
                        processor);

                scheduler.Begin();

                for (var i = 0; i < chunkCount; i++)
                {
                    scheduler.Append(
                        $"word-{i} ");

                    await Task.Delay(interval);
                }

                await scheduler.CompleteAsync();

                var batches =
                    scheduler.ProcessedBatchCount;

                var average =
                    batches == 0
                        ? 0
                        : (double)chunkCount / batches;

                Console.WriteLine(
                    $"{chunkCount,6} | " +
                    $"{interval,8} | " +
                    $"{batches,7} | " +
                    $"{average,11:F2}");
            }
        }
    }

    private static MarkdownStreamProcessor CreateProcessor()
    {
        return new MarkdownStreamProcessor(
            new MarkdownBuffer(),
            new MarkdigMarkdownParser(),
            new DocumentReconciler(),
            new DocumentDiffEngine());
    }
}