using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class DocumentReconcilerBenchmarks
{
    [Params(1_000_000)]
    public int FinalMarkdownSize { get; set; }

    [Params(1_000, 10_000)]
    public int ChunkSize { get; set; }

    private MarkdownDocument _previousDocument = null!;
    private MarkdownDocument _currentDocument = null!;
    private DocumentReconciler _reconciler = null!;
    private int _reusedBlockCount;

    [GlobalSetup]
    public void Setup()
    {
        var markdown =
            GenerateMarkdown(FinalMarkdownSize);

        var chunks =
            SplitIntoChunks(markdown, ChunkSize);

        var parser =
            new IncrementalMarkdownParser(
                new MarkdigMarkdownParser());

        var buffer =
            new StreamingMarkdown.Core.Streaming.MarkdownBuffer();

        _previousDocument =
            MarkdownDocument.Empty;

        foreach (var chunk in chunks)
        {
            buffer.Append(chunk);

            var reparseStart =
                _previousDocument.Blocks.Count > 0
                    ? _previousDocument.Blocks[^1].SourceStart
                    : 0;

            var suffix =
                buffer.GetSuffix(reparseStart);

            var result =
                parser.ParseSuffix(
                    suffix,
                    _previousDocument,
                    reparseStart);

            _currentDocument =
                result.Document;

            _reusedBlockCount =
                result.ReusedBlockCount;

            _previousDocument =
                _currentDocument;
        }

        // Create one representative final incremental update.
        buffer.Append(" additional content");

        var reparseStartFinal =
            _previousDocument.Blocks.Count > 0
                ? _previousDocument.Blocks[^1].SourceStart
                : 0;

        var suffixFinal =
            buffer.GetSuffix(reparseStartFinal);

        var finalResult =
            parser.ParseSuffix(
                suffixFinal,
                _previousDocument,
                reparseStartFinal);

        _currentDocument =
            finalResult.Document;

        _reusedBlockCount =
            finalResult.ReusedBlockCount;

        _reconciler =
            new DocumentReconciler();
    }

    [Benchmark]
    public MarkdownDocument ReconcileIncremental()
    {
        return _reconciler.ReconcileIncremental(
            _previousDocument,
            _currentDocument,
            _reusedBlockCount);
    }

    private static string[] SplitIntoChunks(
        string markdown,
        int chunkSize)
    {
        var chunks = new List<string>();

        for (var start = 0;
             start < markdown.Length;
             start += chunkSize)
        {
            var length =
                Math.Min(
                    chunkSize,
                    markdown.Length - start);

            chunks.Add(
                markdown.Substring(
                    start,
                    length));
        }

        return chunks.ToArray();
    }

    private static string GenerateMarkdown(
        int targetSize)
    {
        var builder =
            new System.Text.StringBuilder(targetSize);

        var paragraph =
            "This is a sample paragraph containing enough Markdown " +
            "content to simulate an AI generated response. ";

        var heading =
            "# Customer Analysis\n\n";

        while (builder.Length < targetSize)
        {
            builder.Append(heading);
            builder.Append(paragraph);
            builder.Append('\n');
        }

        return builder.ToString();
    }
}