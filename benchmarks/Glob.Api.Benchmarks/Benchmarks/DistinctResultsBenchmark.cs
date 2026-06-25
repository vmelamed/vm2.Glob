// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Glob.Api;

/// <summary>
/// Measures the cost of deduplication with Distinct option.
/// Only relevant for patterns with multiple globstars that can produce duplicates.
/// </summary>
public class DistinctResultsBenchmark : BenchmarkBase
{
    const int operationsPerInvoke = 1000;

    [GlobalSetup]
    public void GlobalSetup() => SetupFakeStandardFileSystem();

    [Params("**/docs/**/*.md", "**/test/**/*.cs")]
    public string Pattern { get; set; } = "**/docs/**/*.md";

    [Benchmark(Description = "Non-distinct", OperationsPerInvoke = operationsPerInvoke, Baseline = true)]
    public int NonDistinctResultsTest()
    {
        int suppressOptimizationDiscard = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            suppressOptimizationDiscard = EnumerateAll(
                    new GlobEnumeratorBuilder()
                        .WithGlob(Pattern)
                        .Configure(CreateGlob())
                );
        return suppressOptimizationDiscard;
    }

    [Benchmark(Description = "Distinct", OperationsPerInvoke = operationsPerInvoke)]
    public int DistinctResultsTest()
    {
        int suppressOptimizationDiscard = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            suppressOptimizationDiscard = EnumerateAll(
                new GlobEnumeratorBuilder()
                    .WithGlob(Pattern)
                    .Distinct()
                    .Configure(CreateGlob())
            );
        return suppressOptimizationDiscard;
    }
}
