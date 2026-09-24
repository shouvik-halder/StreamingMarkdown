using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class BackgroundProcessingBenchmarks
{
    [Params(100, 500, 1000, 5000)]
    public int ChunkCount { get; set; }

    private MarkdownStreamScheduler _scheduler = null!;

    private string[] _chunks = null!;

    [GlobalSetup]
    public void Setup()
    {
        var parser =
            new BenchmarkMarkdownParser();

        var processor =
            new MarkdownStreamProcessor(
                new MarkdownBuffer(),
                parser,
                new DocumentReconciler(),
                new DocumentDiffEngine());

        _scheduler =
            new MarkdownStreamScheduler(
                processor);

        _chunks =
            Enumerable
                .Range(0, ChunkCount)
                .Select(i =>
                    $"chunk-{i} ")
                .ToArray();
    }

    [Benchmark]
    public async Task BackgroundProcessing()
    {
        _scheduler.Begin();

        foreach (var chunk in _chunks)
        {
            _scheduler.Append(chunk);
        }

        await _scheduler.CompleteAsync();
    }

    private sealed class BenchmarkMarkdownParser : IMarkdownParser
    {
        public MarkdownDocument Parse(
            string markdown)
        {
            return MarkdownDocument.Empty;
        }
    }
}