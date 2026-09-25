using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class IncrementalParsingBaselineBenchmarks
{
    [Params(
        10_000,
        50_000,
        100_000,
        250_000,
        500_000,
        1_000_000)]
    public int MarkdownSize { get; set; }

    private MarkdigMarkdownParser _parser = null!;
    private string _markdown = null!;

    [GlobalSetup]
    public void Setup()
    {
        _parser = new MarkdigMarkdownParser();

        _markdown = GenerateMarkdown(MarkdownSize);
    }

    [Benchmark]
    public void FullParse()
    {
        _parser.Parse(_markdown);
    }

    private static string GenerateMarkdown(int targetSize)
    {
        var builder = new System.Text.StringBuilder(targetSize);

        var paragraph =
            "This is a sample paragraph containing enough Markdown " +
            "content to simulate an AI generated response. ";

        var heading = "# Customer Analysis\n\n";

        while (builder.Length < targetSize)
        {
            builder.Append(heading);
            builder.Append(paragraph);
            builder.Append('\n');
        }

        return builder.ToString();
    }
}