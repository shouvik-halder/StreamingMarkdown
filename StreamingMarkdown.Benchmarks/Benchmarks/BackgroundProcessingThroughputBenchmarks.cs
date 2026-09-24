using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class BackgroundProcessingThroughputBenchmarks
{
    [Params(100, 500)]
    public int ChunkCount { get; set; }

    [Params(1, 5)]
    public int ProducerIntervalMs { get; set; }

    private MarkdownStreamScheduler _scheduler = null!;

    [Benchmark]
    public async Task StreamAndProcess()
    {
        var processor =
            new MarkdownStreamProcessor(
                new MarkdownBuffer(),
                new SlowMarkdownParser(),
                new DocumentReconciler(),
                new DocumentDiffEngine());

        _scheduler = new MarkdownStreamScheduler(processor);

        _scheduler.Begin();

        for (var i = 0; i < ChunkCount; i++)
        {
            _scheduler.Append($"chunk-{i} ");

            await Task.Delay(ProducerIntervalMs);
        }

        await _scheduler.CompleteAsync();
    }

    private sealed class SlowMarkdownParser : IMarkdownParser
    {
        public MarkdownDocument Parse(string markdown)
        {
            // Simulate expensive parsing/reconciliation work.
            Thread.Sleep(25);

            return MarkdownDocument.Empty;
        }
    }
}