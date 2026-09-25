using BenchmarkDotNet.Running;
using StreamingMarkdown.Benchmarks.Benchmarks;

BenchmarkRunner.Run<IncrementalParserBenchmarks>();

// using StreamingMarkdown.Benchmarks;

// await CoalescingDiagnostics.RunAsync();