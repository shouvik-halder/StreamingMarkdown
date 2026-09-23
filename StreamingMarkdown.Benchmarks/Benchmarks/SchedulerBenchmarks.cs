using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class SchedulerBenchmarks
{
    [Params(100, 500, 1000, 5000)]
    public int ChunkCount { get; set; }

    private string[] _chunks = [];

    [GlobalSetup]
    public void Setup()
    {
        _chunks = Enumerable
            .Range(0, ChunkCount)
            .Select(i => $"word-{i} ")
            .ToArray();
    }

    [Benchmark(Baseline = true)]
    public void DirectProcessor()
    {
        var processor =
            CreateProcessor();

        processor.Begin();

        foreach (var chunk in _chunks)
        {
            processor.Append(chunk);
        }

        processor.Complete();
    }

    [Benchmark]
    public async Task Scheduler()
    {
        var processor =
            CreateProcessor();

        var scheduler =
            new MarkdownStreamScheduler(
                processor);

        scheduler.Begin();

        foreach (var chunk in _chunks)
        {
            scheduler.Append(chunk);
        }

        await scheduler.CompleteAsync();
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