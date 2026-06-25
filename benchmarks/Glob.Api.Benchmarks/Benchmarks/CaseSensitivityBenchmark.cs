// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Glob.Api;

/// <summary>
/// Compares case-sensitive vs case-insensitive matching performance.
/// </summary>
public class CaseSensitivityBenchmark : BenchmarkBase
{
    const int operationsPerInvoke = 1000;

    [GlobalSetup]
    public void GlobalSetup() => SetupFakeStandardFileSystem();

    [Params(
        "**/*.CS",
        "**/*.md")]
    public string Pattern { get; set; } = "**/*.CS";

    [Benchmark(Description = "Case Sensitive", OperationsPerInvoke = operationsPerInvoke)]
    public int CaseSensitiveTest()
    {
        int suppressOptimizationDiscard = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            suppressOptimizationDiscard = EnumerateAll(
                    new GlobEnumeratorBuilder()
                            .WithGlob(Pattern)
                            .CaseSensitive()
                            .Configure(CreateGlob())
                );

        return suppressOptimizationDiscard;
    }

    [Benchmark(Description = "Case Insensitive", OperationsPerInvoke = operationsPerInvoke, Baseline = true)]
    public int CaseInsensitiveTest()
    {
        int suppressOptimizationDiscard = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            suppressOptimizationDiscard = EnumerateAll(
                new GlobEnumeratorBuilder()
                            .WithGlob(Pattern)
                            .CaseInsensitive()
                            .Configure(CreateGlob())
                );

        return suppressOptimizationDiscard;
    }
}
