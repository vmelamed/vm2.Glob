// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Glob.Api;

/// <summary>
/// Compares depth-first vs breadth-first traversal strategies.
/// </summary>
public class TraversalStrategyBenchmark : BenchmarkBase
{
    [GlobalSetup]
    public void GlobalSetup() => SetupFakeStandardFileSystem();

    [Params("**/*.cs", "**/docs/**/*.md")]
    public string Pattern { get; set; } = "**/*.cs";

    [Benchmark(Description = "Traverse Depth First", OperationsPerInvoke = 1000, Baseline = true)]
    public int TdfStrategyTest()
        => EnumerateAll(
                new GlobEnumeratorBuilder()
                    .WithGlob(Pattern)
                    .DepthFirst()
                    .Configure(_glob)
            );

    [Benchmark(Description = "Traverse Breadth First", OperationsPerInvoke = 1000)]
    public int TbfStrategyTest()
        => EnumerateAll(
                new GlobEnumeratorBuilder()
                    .WithGlob(Pattern)
                    .BreadthFirst()
                    .Configure(_glob)
            );
}
