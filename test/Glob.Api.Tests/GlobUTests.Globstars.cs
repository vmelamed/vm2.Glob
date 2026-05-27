// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Tests.Glob.Api;

[ExcludeFromCodeCoverage]
public class GlobstarsTests(GlobUnitTestsFixture fixture, ITestOutputHelper output) : GlobEnumeratorUnitTests(fixture, output)
{
    [Theory]
    [MemberData(nameof(Enumerate_Globstars))]
    public void Should_Enumerate_Globstars_GlobEnumerator(UnitTestElement data) => Enumerate_GlobEnumerator(data);
}
