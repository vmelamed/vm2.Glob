// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Tests.Glob.Api;

[ExcludeFromCodeCoverage]
public class GlobInitialTests(GlobUnitTestsFixture fixture, ITestOutputHelper output) : GlobEnumeratorUnitTests(fixture, output)
{
    [Theory]
    [MemberData(nameof(Enumerate_InitialSet))]
    public void Should_Enumerate_TestDataSet_GlobEnumerator(UnitTestElement data) => Enumerate_GlobEnumerator(data);
}
