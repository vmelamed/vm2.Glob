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
        int a = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            a = EnumerateAll(
                    new GlobEnumeratorBuilder()
                            .WithGlob(Pattern)
                            .CaseSensitive()
                            .Configure(_glob)
                );

        return a;
    }

    [Benchmark(Description = "Case Insensitive", OperationsPerInvoke = operationsPerInvoke, Baseline = true)]
    public int CaseInsensitiveTest()
    {
        int a = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            a = EnumerateAll(
                new GlobEnumeratorBuilder()
                            .WithGlob(Pattern)
                            .CaseInsensitive()
                            .Configure(_glob)
                );

        return a;
    }
}
