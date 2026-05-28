// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Tests.Glob.Api;

[ExcludeFromCodeCoverage]
public class GlobEnumerationDistinctTests(GlobUnitTestsFixture fixture, ITestOutputHelper output) : GlobEnumeratorUnitTests(fixture, output)
{
    [Fact]
    public void Should_Enumerate_WithDuplicates_GlobEnumerator()
    {
        var ge = GetGlobEnumerator(
                            "FSFiles/FS6.Unix.json",
                            builder => builder
                                        .WithGlob("/**/[lb]*/**/[lb]*/*.txt")
                                        .FromDirectory("/")
                                        .CaseSensitive()
                                        .SelectFiles());
        var enumerate = ge.Enumerate;
        var result = enumerate.Should().NotThrow().Which.ToList();
        string[] expected = [
            "/deep-recursive/level1/level2/level3/deep1.txt",
            "/deep-recursive/level1/level2/level3/deep1.txt",
            "/deep-recursive/level1/level2/mid1.txt",
        ];

        Out.WriteLine("Expected Results: \"{0}\"", string.Join("\", \"", expected));
        Out.WriteLine("  Actual Results: \"{0}\"", string.Join("\", \"", result));

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Enumerate_Distinct_GlobEnumerator()
    {
        var ge = GetGlobEnumerator(
                            "FSFiles/FS6.Unix.json",
                            builder => builder
                                        .WithGlob("/**/[lb]*/**/[lb]*/*.txt")
                                        .FromDirectory("/")
                                        .CaseInsensitive()
                                        .SelectFiles()
                                        .Distinct()
                            );
        var enumerate = ge.Enumerate;
        var result = enumerate.Should().NotThrow().Which.ToList();
        string[] expected = [
            "/deep-recursive/level1/level2/level3/deep1.txt",
            "/deep-recursive/level1/level2/mid1.txt",
        ];

        Out.WriteLine("Expected Results: \"{0}\"", string.Join("\", \"", expected));
        Out.WriteLine("  Actual Results: \"{0}\"", string.Join("\", \"", result));

        result.Should().BeEquivalentTo(expected);
    }
}
