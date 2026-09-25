using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class ProgressiveIncrementalParsingBenchmarks
{
    [Params(1_000_000)]
    public int FinalMarkdownSize { get; set; }

    [Params(1_000, 10_000)]
    public int ChunkSize { get; set; }

    private MarkdownStreamProcessor _processor = null!;
    private string[] _chunks = null!;

    [GlobalSetup]
    public void Setup()
    {
        var markdown =
            GenerateMarkdown(FinalMarkdownSize);

        _chunks = SplitIntoChunks(
            markdown,
            ChunkSize);

        _processor =
            new MarkdownStreamProcessor(
                new MarkdownBuffer(),
                new MarkdigMarkdownParser(),
                new DocumentReconciler(),
                new DocumentDiffEngine());
    }

    [Benchmark]
    public void ProgressiveIncrementalParse()
    {
        _processor.Begin();

        foreach (var chunk in _chunks)
        {
            _processor.Append(chunk);
        }

        _processor.Complete();
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