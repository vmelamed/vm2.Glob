// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Glob.Api;

/// <summary>
/// Benchmarks performance across different test structure sizes.
/// </summary>
public class StructureSizeBenchmark : BenchmarkBase
{
    const string FsLargeJsonModelFileName = "large-test-tree.json";

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

    [Benchmark(Description = "Small File System", Baseline = true)]
    public int SmallFileSystemTest()
        => EnumerateAll(
                new GlobEnumeratorBuilder()
                        .WithGlob(Pattern)
                        .Configure(_glob)
            );

    [Benchmark(Description = "Large File System")]
    public int LargeFileSystemTest()
        => EnumerateAll(
                new GlobEnumeratorBuilder()
                        .WithGlob(Pattern)
                        .Configure(_globLarge)
            );
}
