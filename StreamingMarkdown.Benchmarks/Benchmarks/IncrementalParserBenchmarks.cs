using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Models;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class IncrementalParserBenchmarks
{
    [Params(1_000_000)]
    public int FinalMarkdownSize { get; set; }

    [Params(1_000, 10_000)]
    public int ChunkSize { get; set; }

    private IncrementalMarkdownParser _parser = null!;
    private string _markdown = null!;
    private string[] _chunks = null!;

    [GlobalSetup]
    public void Setup()
    {
        _markdown =
            GenerateMarkdown(FinalMarkdownSize);

        _chunks =
            SplitIntoChunks(
                _markdown,
                ChunkSize);

        _parser =
            new IncrementalMarkdownParser(
                new MarkdigMarkdownParser());
    }

    [Benchmark]
    public void ProgressiveIncrementalParse()
    {
        var document =
            MarkdownDocument.Empty;

        var buffer =
            new System.Text.StringBuilder(
                _markdown.Length);

        foreach (var chunk in _chunks)
        {
            buffer.Append(chunk);

            var markdown =
                buffer.ToString();

            var result =
                _parser.Parse(
                    markdown,
                    document);

            document =
                result.Document;
        }
    }

    [Benchmark]
    public void ProgressiveIncrementalParseSuffix()
    {
        var document =
            MarkdownDocument.Empty;

        var buffer =
            new MarkdownBuffer();

        foreach (var chunk in _chunks)
        {
            buffer.Append(chunk);

            var reparseStart =
                document.Blocks.Count > 0
                    ? document.Blocks[^1].SourceStart
                    : 0;

            var suffix =
                buffer.GetSuffix(reparseStart);

            var result =
                _parser.ParseSuffix(
                    suffix,
                    document,
                    reparseStart);

            document =
                result.Document;
        }
    }

    [Benchmark]
    public void ParseFixedSuffix()
    {
        _parser.ParseSuffix(
            _chunks[_chunks.Length / 2],
            MarkdownDocument.Empty,
            0);
    }

    [Benchmark]
    public void ParseSuffixWithGrowingDocument()
    {
        var document =
            MarkdownDocument.Empty;

        var buffer =
            new MarkdownBuffer();

        foreach (var chunk in _chunks)
        {
            buffer.Append(chunk);

            var reparseStart =
                document.Blocks.Count > 0
                    ? document.Blocks[^1].SourceStart
                    : 0;

            var suffix =
                buffer.GetSuffix(reparseStart);

            var result =
                _parser.ParseSuffix(
                    suffix,
                    document,
                    reparseStart);

            document =
                result.Document;
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