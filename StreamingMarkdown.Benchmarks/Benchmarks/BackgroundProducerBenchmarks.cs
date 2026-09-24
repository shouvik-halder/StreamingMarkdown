using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class BackgroundProducerBenchmarks
{
    [Params(100, 500, 1000)]
    public int ChunkCount { get; set; }

    private MarkdownStreamScheduler _scheduler = null!;

    [IterationSetup]
    public void IterationSetup()
    {
        var processor =
            new MarkdownStreamProcessor(
                new MarkdownBuffer(),
                new SlowMarkdownParser(),
                new DocumentReconciler(),
                new DocumentDiffEngine());

        _scheduler =
            new MarkdownStreamScheduler(processor);

        _scheduler.Begin();
    }

    [Benchmark]
    public void AppendBurst()
    {
        for (var i = 0; i < ChunkCount; i++)
        {
            _scheduler.Append(
                $"chunk-{i} ");
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _scheduler.Reset();
    }

    private sealed class SlowMarkdownParser : IMarkdownParser
    {
        public MarkdownDocument Parse(
            string markdown)
        {
            Thread.Sleep(5);

            return MarkdownDocument.Empty;
        }
    }
}