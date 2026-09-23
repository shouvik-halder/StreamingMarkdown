using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Models.Blocks;
using StreamingMarkdown.Core.Models.Inlines;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class DiffBenchmarks
{
    private DocumentDiffEngine _diffEngine = null!;
    private MarkdownDocument _previous = null!;
    private MarkdownDocument _current = null!;

    [Params(10, 100, 500, 1000)]
    public int BlockCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _diffEngine = new DocumentDiffEngine();

        _previous = CreateDocument(BlockCount);
        _current = CreateModifiedDocument(BlockCount);
    }

    [Benchmark]
    public MarkdownDiff Compare()
    {
        return _diffEngine.Compare(
            _previous,
            _current);
    }

    private static MarkdownDocument CreateDocument(int blockCount)
    {
        var blocks = new List<MarkdownBlock>(blockCount);

        for (var i = 0; i < blockCount; i++)
        {
            blocks.Add(
                new ParagraphBlock(
                    i,
                    new MarkdownInline[]
                    {
                        new TextInline($"Paragraph {i}")
                    }));
        }

        return new MarkdownDocument(blocks);
    }

    private static MarkdownDocument CreateModifiedDocument(int blockCount)
    {
        var blocks = new List<MarkdownBlock>(blockCount);

        for (var i = 0; i < blockCount; i++)
        {
            blocks.Add(
                new ParagraphBlock(
                    i,
                    new MarkdownInline[]
                    {
                        new TextInline($"Updated paragraph {i}")
                    }));
        }

        return new MarkdownDocument(blocks);
    }
}
