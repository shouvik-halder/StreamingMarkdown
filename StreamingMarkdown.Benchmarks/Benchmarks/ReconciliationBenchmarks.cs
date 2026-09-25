using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class ReconciliationBenchmarks
{
    private DocumentReconciler _reconciler = null!;
    private MarkdownDocument _previous = null!;
    private MarkdownDocument _current = null!;

    [Params(10, 100, 500, 1000)]
    public int BlockCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _reconciler =
            new DocumentReconciler();

        _previous =
            CreateDocument(BlockCount);

        _current =
            CreateModifiedDocument(BlockCount);
    }

    [Benchmark(Baseline = true)]
    public MarkdownDocument Reconcile()
    {
        return _reconciler.Reconcile(
            _previous,
            _current);
    }

    [Benchmark]
    public MarkdownDocument ReconcileIncremental()
    {
        // Treat every block except the final block
        // as the stable prefix.
        var reusedBlockCount =
            Math.Max(
                0,
                BlockCount - 1);

        return _reconciler.ReconcileIncremental(
            _previous,
            _current,
            reusedBlockCount);
    }

    private static MarkdownDocument CreateDocument(
        int blockCount)
    {
        var blocks =
            new List<MarkdownBlock>(
                blockCount);

        for (var i = 0;
             i < blockCount;
             i++)
        {
            blocks.Add(
                new ParagraphBlock(
                    i,
                    i,
                    i + 1,
                    [
                        new TextInline(
                            $"Paragraph {i}")
                    ]));
        }

        return new MarkdownDocument(
            blocks);
    }

    private static MarkdownDocument CreateModifiedDocument(
        int blockCount)
    {
        var blocks =
            new List<MarkdownBlock>(
                blockCount);

        for (var i = 0;
             i < blockCount;
             i++)
        {
            blocks.Add(
                new ParagraphBlock(
                    i,
                    i,
                    i + 1,
                    [
                        new TextInline(
                            $"Updated paragraph {i}")
                    ]));
        }

        return new MarkdownDocument(
            blocks);
    }
}