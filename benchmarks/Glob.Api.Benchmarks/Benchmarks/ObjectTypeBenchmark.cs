// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Glob.Api;

/// <summary>
/// Compares performance of enumerating files vs directories vs both.
/// </summary>
public class ObjectTypeBenchmark : BenchmarkBase
{
    const int operationsPerInvoke = 1000;

    [GlobalSetup]
    public void GlobalSetup() => SetupFakeStandardFileSystem();

    [Params("**/*", "**/test/**/*")]
    public string Pattern { get; set; } = "**/*";

    [Benchmark(Description = "Get Files", OperationsPerInvoke = operationsPerInvoke, Baseline = true)]
    public int FilesTest()
    {
        int suppressOptimizationDiscard = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            suppressOptimizationDiscard = EnumerateAll(
                new GlobEnumeratorBuilder()
                    .WithGlob(Pattern)
                    .SelectFiles()
                    .Configure(_glob)
            );
        return suppressOptimizationDiscard;
    }

    [Benchmark(Description = "Get Directories", OperationsPerInvoke = operationsPerInvoke)]
    public int DirectoriesTest()
    {
        int suppressOptimizationDiscard = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            suppressOptimizationDiscard = EnumerateAll(
                new GlobEnumeratorBuilder()
                    .WithGlob(Pattern)
                    .SelectDirectories()
                    .Configure(_glob)
            );

        return suppressOptimizationDiscard;
    }

    [Benchmark(Description = "Get Files and Directories", OperationsPerInvoke = operationsPerInvoke)]
    public int DirectoriesAndFilesTest()
    {
        int suppressOptimizationDiscard = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            suppressOptimizationDiscard = EnumerateAll(
                new GlobEnumeratorBuilder()
                    .WithGlob(Pattern)
                    .SelectDirectoriesAndFiles()
                    .Configure(_glob)
            );
        return suppressOptimizationDiscard;
    }
}
