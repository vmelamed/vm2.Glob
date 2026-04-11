// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Glob.Api.Tests;

[ExcludeFromCodeCoverage]
public abstract partial class GlobEnumeratorUnitTests(ITestOutputHelper output) : TestBase(output), IClassFixture<GlobUnitTestsFixture>
{
    protected IHost _host = null!;

    protected GlobUnitTestsFixture Fixture { get; } = null!;

    public GlobEnumeratorUnitTests(
        GlobUnitTestsFixture fixture,
        ITestOutputHelper output) : this(output)
    {
        Fixture = fixture;

        _host = Fixture.BuildHost(output);
    }

    protected GlobEnumerator GetGlobEnumerator(
        string fileSystemDescriptionFile)
        => _host.Services.GetGlobEnumerator(fileSystemDescriptionFile);

    protected GlobEnumerator GetGlobEnumerator(
        string fileSystemDescriptionFile,
        Func<GlobEnumeratorBuilder, GlobEnumeratorBuilder> configureBuilder)
        => _host.Services.GetGlobEnumerator(configureBuilder, fileSystemDescriptionFile);

    protected virtual void Enumerate_GlobEnumerator(UnitTestElement data)
    {
        // Arrange
        var ge = GetGlobEnumerator(data.Fs, data.ConfigureBuilder);
        var enumerate = ge.Enumerate;

        if (data.Throws)
        {
            // Act & Assert
            enumerate.Enumerating().Should().Throw<ArgumentException>();
            return;
        }

        // Act
        var result = enumerate
                        .Should()
                        .NotThrow()
                        .Which
                        .ToList()
                        ;

        Out.WriteLine("Expected Results: \"{0}\"", string.Join("\", \"", data.R));
        Out.WriteLine("  Actual Results: \"{0}\"", string.Join("\", \"", result));

        // Assert
        result.Should().BeEquivalentTo(data.R);
    }

    protected static GlobEnumeratorBuilder CreateBuilder(
        UnitTestElement data,
        bool distinct = false) => ((GlobEnumeratorBuilder)data).WithDistinct(distinct);
}
