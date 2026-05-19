// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Glob.Api.Tests;

using vm2.Glob.Api.DI;

[ExcludeFromCodeCoverage]
public sealed class GlobIntegrationTestsFixture : GlobUnitTestsFixture
{
    public override IHost BuildHost(ITestOutputHelper testOutputHelper)
    {
        var builder = CreateHostApplicationBuilder();

        builder
            .Configuration
            .Sources
            .Clear()
            ;
        builder
            .Configuration
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("USERNAME")}.json", optional: true)
            .AddEnvironmentVariables()
            ;
        builder
            .Logging
            .ClearProviders()
            .AddConsole()
            .AddJsonConsole()
            .SetMinimumLevel(LogLevel.Trace)
        ;
        builder
            .Services
            .AddScoped(sp => testOutputHelper)
            .AddScoped<ILoggerProvider, XUnitLoggerProvider>()
            .AddGlobEnumerator()                      // for the unit tests
            ;

        return builder.Build();
    }
}
