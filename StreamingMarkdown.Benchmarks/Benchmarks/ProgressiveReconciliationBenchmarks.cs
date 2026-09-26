using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class ProgressiveReconciliationBenchmarks
{
    [Params(1_000_000)]
    public int FinalMarkdownSize { get; set; }

    [Params(1_000, 10_000)]
    public int ChunkSize { get; set; }

    private string[] _chunks = null!;
    private IncrementalMarkdownParser _parser = null!;
    private DocumentReconciler _reconciler = null!;

    [GlobalSetup]
    public void Setup()
    {
        var markdown =
            GenerateMarkdown(FinalMarkdownSize);

        _chunks =
            SplitIntoChunks(
                markdown,
                ChunkSize);

        _parser =
            new IncrementalMarkdownParser(
                new MarkdigMarkdownParser());

        _reconciler =
            new DocumentReconciler();
    }

    [Benchmark]
    public void ProgressiveReconciliation()
    {
        var buffer =
            new MarkdownBuffer();

        var document =
            MarkdownDocument.Empty;

        foreach (var chunk in _chunks)
        {
            buffer.Append(chunk);

            var reparseStart =
                document.Blocks.Count > 0
                    ? document.Blocks[^1].SourceStart
                    : 0;

            var suffix =
                buffer.GetSuffix(
                    reparseStart);

            var parseResult =
                _parser.ParseSuffix(
                    suffix,
                    document,
                    reparseStart);

            document =
                _reconciler.ReconcileIncremental(
                    document,
                    parseResult.Document,
                    parseResult.ReusedBlockCount);
        }
    }

    private static string[] SplitIntoChunks(
        string markdown,
        int chunkSize)
    {
        var chunks =
            new List<string>();

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
            new System.Text.StringBuilder(
                targetSize);

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