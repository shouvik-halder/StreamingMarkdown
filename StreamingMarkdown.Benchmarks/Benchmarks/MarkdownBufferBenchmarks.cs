using BenchmarkDotNet.Attributes;
using StreamingMarkdown.Core.Streaming;

namespace StreamingMarkdown.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class MarkdownBufferBenchmarks
{
    private MarkdownBuffer _buffer = null!;

    [Params(100, 500, 1000, 10000)]
    public int ChunkCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _buffer = new MarkdownBuffer();
    }

    [Benchmark]
    public void AppendChunks()
    {
        _buffer = new MarkdownBuffer();

        for (var i = 0; i < ChunkCount; i++)
        {
            _buffer.Append($"This is streamed chunk {i}. ");
        }
    }
}