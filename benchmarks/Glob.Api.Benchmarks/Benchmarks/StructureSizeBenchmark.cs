// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Glob.Api;

/// <summary>
/// Benchmarks performance across different test structure sizes.
/// </summary>
public class StructureSizeBenchmark : BenchmarkBase
{
    const string FsLargeJsonModelFileName = "large-test-tree.json";
    const int operationsPerInvoke = 1000;

    protected string _fsLargeJsonModelPath = null!;
    GlobEnumerator _globLarge = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // create the standard glob enumerator:
        SetupFakeStandardFileSystem();

        // create the large glob enumerator:
        _fsLargeJsonModelPath = Path.Combine(
                                            BmConfiguration.Options.FsJsonModelsDirectory,
                                            FsLargeJsonModelFileName);
        _globLarge = SetupFakeFileSystem(_fsLargeJsonModelPath);
    }

    [Params("**/*.cs", "**/*.md")]
    public string Pattern { get; set; } = "**/*.cs";

    [Benchmark(Description = "Small File System", OperationsPerInvoke = operationsPerInvoke, Baseline = true)]
    public int SmallFileSystemTest()
    {
        int a = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            a = EnumerateAll(
                new GlobEnumeratorBuilder()
                        .WithGlob(Pattern)
                        .Configure(_glob)
            );
        return a;
    }

    [Benchmark(Description = "Large File System", OperationsPerInvoke = operationsPerInvoke)]
    public int LargeFileSystemTest()
    {
        int a = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            a = EnumerateAll(
                new GlobEnumeratorBuilder()
                        .WithGlob(Pattern)
                        .Configure(_globLarge)
            );
        return a;
    }
}
