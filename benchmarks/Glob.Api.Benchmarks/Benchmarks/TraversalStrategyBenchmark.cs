// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Glob.Api;

/// <summary>
/// Compares depth-first vs breadth-first traversal strategies.
/// </summary>
public class TraversalStrategyBenchmark : BenchmarkBase
{
    const int operationsPerInvoke = 1000;

    [GlobalSetup]
    public void GlobalSetup() => SetupFakeStandardFileSystem();

    [Params("**/*.cs", "**/docs/**/*.md")]
    public string Pattern { get; set; } = "**/*.cs";

    [Benchmark(Description = "Traverse Depth First", OperationsPerInvoke = operationsPerInvoke, Baseline = true)]
    public int TdfStrategyTest()
    {
        int a = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            a = EnumerateAll(
                new GlobEnumeratorBuilder()
                    .WithGlob(Pattern)
                    .DepthFirst()
                    .Configure(_glob)
            );
            
        return a;
    }

    [Benchmark(Description = "Traverse Breadth First", OperationsPerInvoke = operationsPerInvoke)]
    public int TbfStrategyTest()
    {
        int a = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            a = EnumerateAll(
                new GlobEnumeratorBuilder()
                    .WithGlob(Pattern)
                    .BreadthFirst()
                    .Configure(_glob)
            );
        return a;
    }
}
