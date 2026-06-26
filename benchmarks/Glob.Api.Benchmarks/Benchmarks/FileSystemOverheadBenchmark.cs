// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Glob.Api;

/// <summary>
/// Benchmarks to measure the overhead of real filesystem vs in-memory FakeFS.
/// This provides baseline measurements to understand I/O impact.
/// </summary>
public class FileSystemBenchmark : BenchmarkBase
{
    const int operationsPerInvoke = 1000;

    GlobContext _globRealFS;

    [GlobalSetup]
    public void GlobalSetup()
    {
        SetupFakeStandardFileSystem();
        _globRealFS = SetupRealFileSystems(_fsStandardJsonModelPath);
    }

    [GlobalCleanup]
    public virtual void GlobalCleanup() => CleanupRealFileSystems();

    [Params("**/*.cs", "**/*.md", "**/test/**/*.cs")]
    public string Pattern { get; set; } = "**/*.cs";

    [Benchmark(Description = "Fake File System Base", OperationsPerInvoke = operationsPerInvoke, Baseline = true)]
    public int FakeFileSystemTest()
    {
        int suppressOptimizationDiscard = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            suppressOptimizationDiscard = EnumerateAll(
                new GlobEnumeratorBuilder()
                        .WithGlob(Pattern)
                        .Configure(CreateGlob()));
        return suppressOptimizationDiscard;
    }

    [Benchmark(Description = "Real File System Overhead", OperationsPerInvoke = operationsPerInvoke)]
    public int RealFileSystemTest()
    {
        int suppressOptimizationDiscard = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            suppressOptimizationDiscard = EnumerateAll(
                new GlobEnumeratorBuilder()
                        .WithGlob(Pattern)
                        .Configure(CreateGlob(_globRealFS)));
        return suppressOptimizationDiscard;
    }
}
