using BenchmarkDotNet.Running;
using StreamingMarkdown.Benchmarks.Benchmarks;

BenchmarkRunner.Run<StreamProcessorIncrementalBenchmarks>();

// using StreamingMarkdown.Benchmarks;

// await CoalescingDiagnostics.RunAsync();