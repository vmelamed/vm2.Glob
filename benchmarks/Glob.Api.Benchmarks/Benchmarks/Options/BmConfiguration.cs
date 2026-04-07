// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed


namespace vm2.Glob.Api.Benchmarks.Options;

public static class BmConfiguration
{
    public static BmOptions Options { get; private set; } = new();

    public static void BindOptions()
    {
        var builder = new ConfigurationBuilder();

        builder
            .Sources
            .Clear();

        var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            ;

        if (env == "Development")
            builder.AddJsonFile("appsettings.Development.json", optional: true);

        if (env == "Staging")
            builder.AddJsonFile("appsettings.Staging.json", optional: true);

        builder
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("USERNAME")}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(Environment.GetCommandLineArgs())
            .Build()
            .GetSection(nameof(BmOptions))
            .Bind(Options)
            ;

        Options = new BmOptions(
            TestFileStructure.ExpandEnvironmentVariables(Options.ResultsPath),
            TestFileStructure.ExpandEnvironmentVariables(Options.FsJsonModelsDirectory),
            TestFileStructure.ExpandEnvironmentVariables(Options.TestsRootPath)
        );
    }
}
