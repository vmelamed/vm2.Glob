// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Glob.Api.Tests;

using vm2.Glob.Api.DI;

[ExcludeFromCodeCoverage]
public sealed class GlobIntegrationTestsFixture : GlobUnitTestsFixture
{
    public override IHost BuildHost(ITestOutputHelper testOutputHelper)
    {
        // *TODO*: Remove this workaround after the broken C# Dev Kit / test-host
        // release stops creating file watchers during default host initialization.
        // ==> var builder = Host.CreateApplicationBuilder();
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });

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
