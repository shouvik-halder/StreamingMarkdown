using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Parsing;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class ProgressiveParsingBaselineBenchmarks
{
    [Params(100_000, 500_000, 1_000_000)]
    public int FinalMarkdownSize { get; set; }

    [Params(1_000, 5_000, 10_000)]
    public int ChunkSize { get; set; }

    private MarkdigMarkdownParser _parser = null!;
    private string[] _documents = null!;

    [GlobalSetup]
    public void Setup()
    {
        _parser = new MarkdigMarkdownParser();

        var finalMarkdown = GenerateMarkdown(FinalMarkdownSize);

        var documents = new List<string>();

        for (var length = ChunkSize;
             length < finalMarkdown.Length;
             length += ChunkSize)
        {
            documents.Add(finalMarkdown[..length]);
        }

        documents.Add(finalMarkdown);

        _documents = documents.ToArray();
    }

    [Benchmark]
    public void ProgressiveFullParse()
    {
        foreach (var document in _documents)
        {
            _parser.Parse(document);
        }
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