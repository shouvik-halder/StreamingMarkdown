using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class ParserBenchmarks
{
    private MarkdigMarkdownParser _parser = null!;
    private string _markdown = null!;

    [Params(10_000, 100_000, 500_000, 1_000_000)]
    public int CharacterCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _parser = new MarkdigMarkdownParser();

        _markdown = GenerateMarkdown(CharacterCount);
    }

    private static string GenerateMarkdown(int characterCount)
    {
        const string paragraph =
            "This is a paragraph containing **bold text**, " +
            "*italic text*, and a [link](https://example.com).\n\n";

        var builder = new System.Text.StringBuilder(characterCount);

        while (builder.Length < characterCount)
        {
            builder.Append(paragraph);
        }

        return builder.ToString();
    }

    [Benchmark]
    public void Parse()
    {
        _parser.Parse(_markdown);
    }
}