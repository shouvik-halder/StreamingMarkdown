using BenchmarkDotNet.Running;
using StreamingMarkdown.Benchmarks.Benchmarks;

BenchmarkRunner.Run<BackgroundProcessingThroughputBenchmarks>();

// using StreamingMarkdown.Benchmarks;

// await CoalescingDiagnostics.RunAsync();