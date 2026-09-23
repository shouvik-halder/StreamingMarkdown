using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Benchmarks.TestData;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class StreamProcessorBenchmarks
{
    private string[] _smallChunks = [];
    private string[] _normalChunks = [];
    private string[] _markdownChunks = [];

    [Params(100, 500, 1000)]
    public int ChunkCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _smallChunks =
            CreateChunks(
                StreamingMarkdownSamples.SimpleMarkdown,
                10);

        _normalChunks =
            CreateChunks(
                StreamingMarkdownSamples.SimpleMarkdown,
                50);

        _markdownChunks =
            CreateChunks(
                StreamingMarkdownSamples.MarkdownHeavy,
                20);
    }

    [Benchmark]
    public void ProcessSmallChunks()
    {
        var processor = CreateProcessor();

        processor.Begin();

        foreach (var chunk in _smallChunks)
        {
            processor.Append(chunk);
        }

        processor.Complete();
    }

    [Benchmark]
    public void ProcessNormalChunks()
    {
        var processor = CreateProcessor();

        processor.Begin();

        foreach (var chunk in _normalChunks)
        {
            processor.Append(chunk);
        }

        processor.Complete();
    }

    [Benchmark]
    public void ProcessMarkdownChunks()
    {
        var processor = CreateProcessor();

        processor.Begin();

        foreach (var chunk in _markdownChunks)
        {
            processor.Append(chunk);
        }

        processor.Complete();
    }

    [Benchmark]
    public void ProcessProgressiveStream()
    {
        var processor = CreateProcessor();

        processor.Begin();

        for (var i = 0; i < ChunkCount; i++)
        {
            processor.Append(
                $"This is streamed chunk {i}. ");
        }

        processor.Complete();
    }

    private static MarkdownStreamProcessor CreateProcessor()
    {
        return new MarkdownStreamProcessor(
            new MarkdownBuffer(),
            new MarkdigMarkdownParser(),
            new DocumentReconciler(),
            new DocumentDiffEngine());
    }

    private static string[] CreateChunks(
        string input,
        int chunkSize)
    {
        var chunks = new List<string>();

        for (var i = 0; i < input.Length; i += chunkSize)
        {
            var length =
                Math.Min(
                    chunkSize,
                    input.Length - i);

            chunks.Add(
                input.Substring(
                    i,
                    length));
        }

        return chunks.ToArray();
    }
}