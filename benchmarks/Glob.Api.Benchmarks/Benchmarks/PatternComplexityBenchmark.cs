// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Benchmarks.Glob.Api;

/// <summary>
/// Benchmarks different glob pattern complexities to identify performance characteristics.
/// </summary>
public class PatternComplexityBenchmark : BenchmarkBase
{
    const int operationsPerInvoke = 1000;

    [GlobalSetup]
    public void GlobalSetup() => SetupFakeStandardFileSystem();

    [Params(
        "*.md",                      // Simple: root only
        "src/*.cs",                  // Single level
        "**/*.cs",                   // Single globstar
        "**/*.md",                   // Single globstar (different extension)
        "**/test/**/*.cs",           // Multiple globstars
        "**/docs/**/*.md",           // Multiple globstars (different path)
        "**/?????Service.cs",        // Character wildcard
        "**/test/**/???Tests.cs"     // Mixed wildcards
    )]
    public string Pattern { get; set; } = "*.md";

    [Benchmark(Description = "Pattern Complexity", OperationsPerInvoke = operationsPerInvoke)]
    public int PatternComplexityTest()
    {
        int suppressOptimizationDiscard = 0;

        for (int i = 0; i < operationsPerInvoke; i++)
            suppressOptimizationDiscard = EnumerateAll(
                    new GlobEnumeratorBuilder()
                        .WithGlob(Pattern)
                        .Configure(_glob)
                );
        return suppressOptimizationDiscard;
    }
}
