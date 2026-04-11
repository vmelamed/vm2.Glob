// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Glob.Api.Tests;

[ExcludeFromCodeCoverage]
public class GlobEnumerationOrderTests(GlobUnitTestsFixture fixture, ITestOutputHelper output) : GlobEnumeratorUnitTests(fixture, output)
{
    [Fact]
    public void Should_Enumerate_DepthFirst_GlobEnumerator()
    {
        var ge = GetGlobEnumerator(
                            "FSFiles/FS7.Unix.json",
                            builder => builder
                                        .WithGlob("**/*.txt")
                                        .FromDirectory("/")
                                        .CaseSensitive()
                                        .SelectFiles()
                                        .DepthFirst()
                                        .Distinct()
                            );
        var enumerate = ge.Enumerate;
        var result = enumerate.Should().NotThrow().Which.ToList();
        string[] expected = [
            "/aaa.txt",
            "/a/aa.txt",
            "/a/b/bb.txt",
            "/a/b/c/cc.txt",
            "/x/xx.txt",
            "/x/y/yy.txt",
            "/x/y/z/zz.txt",
        ];

        Out.WriteLine("Expected Results:\n    \"{0}\"", string.Join("\",\n    \"", expected));
        Out.WriteLine("Actual Results:\n    \"{0}\"", string.Join("\",\n    \"", result));

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Should_Enumerate_BreadthFirst_GlobEnumerator()
    {
        var ge = GetGlobEnumerator(
                            "FSFiles/FS7.Unix.json",
                            builder => builder
                                        .WithGlob("**/*.txt")
                                        .FromDirectory("/")
                                        .CaseInsensitive()
                                        .SelectFiles()
                                        .BreadthFirst()
                                        .Distinct()
                            );
        var enumerate = ge.Enumerate;
        var result = enumerate.Should().NotThrow().Which.ToList();
        string[] expected = [
            "/aaa.txt",
            "/a/aa.txt",
            "/x/xx.txt",
            "/a/b/bb.txt",
            "/x/y/yy.txt",
            "/a/b/c/cc.txt",
            "/x/y/z/zz.txt",
        ];

        Out.WriteLine("Expected Results:\n    \"{0}\"", string.Join("\",\n    \"", expected));
        Out.WriteLine("Actual Results:\n    \"{0}\"", string.Join("\",\n    \"", result));

        result.Should().BeEquivalentTo(expected);
    }
}
