StreamingMarkdown V1 Performance Baseline
------------------------------------------

Environment:
Apple M3 Pro
.NET 10.0.8
BenchmarkDotNet 0.15.8
Release
Arm64

Buffer:
1,000 chunks
15.731 μs
141.94 KB

Parser:
1 MB
31.517 ms
35,663.65 KB

Reconciler:
1,000 blocks
3.731 ms
120.34 KB

Diff:
1,000 blocks
5.793 ms
3,977.87 KB

Token Stream:
5,000 chunks
21.130 ms
251,244.59 KB