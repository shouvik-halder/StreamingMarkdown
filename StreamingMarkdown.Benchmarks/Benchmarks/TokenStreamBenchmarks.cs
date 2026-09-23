using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Diffing;
using StreamingMarkdown.Core.Parsing;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class TokenStreamBenchmarks
{
    private string[] _chunks = null!;

    [Params(100, 500, 1000, 5000)]
    public int ChunkCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _chunks = new string[ChunkCount];

        for (var i = 0; i < ChunkCount; i++)
        {
            _chunks[i] = "word ";
        }
    }

    [Benchmark]
    public void ProcessTokenStream()
    {
        var processor =
            new MarkdownStreamProcessor(
                new MarkdownBuffer(),
                new MarkdigMarkdownParser(),
                new DocumentReconciler(),
                new DocumentDiffEngine());

        processor.Begin();

        for (var i = 0; i < _chunks.Length; i++)
        {
            processor.Append(_chunks[i]);
        }

        processor.Complete();
    }
}