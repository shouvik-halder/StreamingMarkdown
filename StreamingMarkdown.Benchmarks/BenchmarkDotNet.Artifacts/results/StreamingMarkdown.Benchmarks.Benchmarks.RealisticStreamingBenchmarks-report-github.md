```

BenchmarkDotNet v0.15.8, macOS 27.0 (26A428) [Darwin 27.0.0]
Apple M3 Pro, 1 CPU, 11 logical and 11 physical cores
.NET SDK 10.0.300
  [Host]     : .NET 10.0.8 (10.0.8, 10.0.826.23019), Arm64 RyuJIT armv8.0-a
  DefaultJob : .NET 10.0.8 (10.0.8, 10.0.826.23019), Arm64 RyuJIT armv8.0-a


```
| Method                   | ChunkCount | ChunkIntervalMs | Mean        | Error   | StdDev  | Allocated  |
|------------------------- |----------- |---------------- |------------:|--------:|--------:|-----------:|
| **SchedulerRealisticStream** | **100**        | **1**               |    **116.3 ms** | **1.62 ms** | **1.44 ms** |   **73.35 KB** |
| **SchedulerRealisticStream** | **100**        | **5**               |    **562.9 ms** | **3.40 ms** | **3.18 ms** |  **161.85 KB** |
| **SchedulerRealisticStream** | **100**        | **10**              |  **1,074.8 ms** | **3.66 ms** | **3.24 ms** |  **258.87 KB** |
| **SchedulerRealisticStream** | **100**        | **20**              |  **2,074.0 ms** | **4.59 ms** | **4.29 ms** |   **379.8 KB** |
| **SchedulerRealisticStream** | **500**        | **1**               |    **580.6 ms** | **6.80 ms** | **6.36 ms** |   **526.8 KB** |
| **SchedulerRealisticStream** | **500**        | **5**               |  **2,804.6 ms** | **6.59 ms** | **6.16 ms** |  **1898.3 KB** |
| **SchedulerRealisticStream** | **500**        | **10**              |  **5,378.7 ms** | **8.99 ms** | **8.41 ms** | **3476.32 KB** |
| **SchedulerRealisticStream** | **500**        | **20**              | **10,378.2 ms** | **9.03 ms** | **8.44 ms** | **5259.76 KB** |
