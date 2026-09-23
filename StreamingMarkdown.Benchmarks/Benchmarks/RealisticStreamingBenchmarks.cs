using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class RealisticStreamingBenchmarks
{
    [Params(100, 500)]
    public int ChunkCount { get; set; }

    [Params(1, 5, 10, 20)]
    public int ChunkIntervalMs { get; set; }

    private string[] _chunks = [];

    [GlobalSetup]
    public void Setup()
    {
        _chunks = Enumerable
            .Range(0, ChunkCount)
            .Select(i => $"word-{i} ")
            .ToArray();
    }

    [Benchmark]
    public async Task SchedulerRealisticStream()
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

            await Task.Delay(
                ChunkIntervalMs);
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