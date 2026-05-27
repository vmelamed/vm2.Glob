// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Tests.Glob.Api;

[ExcludeFromCodeCoverage]
public class GlobUnitTestsFixture : IDisposable
{
    // *TODO*: Remove this workaround after the broken C# Dev Kit / test-host
    // release stops creating file watchers during default host initialization.
    // ==> return Host.CreateApplicationBuilder();
    protected static HostApplicationBuilder CreateHostApplicationBuilder()
        => Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { DisableDefaults = true });

    public virtual IHost BuildHost(ITestOutputHelper testOutputHelper)
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
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("USERPROFILE")}.json", optional: true)
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
            .AddGlobEnumeratorFactory()                      // for the unit tests
            ;

        return builder.Build();
    }

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
