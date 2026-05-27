// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

global using System.Configuration;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;

global using vm2.Glob.Api;
global using vm2.Glob.Api.DI;
global using vm2.Glob.Api.FakeFileSystem;
global using vm2.Glob.Api.FakeFileSystem.DI;
global using vm2.TestUtilities;
global using vm2.TestUtilities.XUnitLogger;

global using Xunit.Sdk;

global using static vm2.TestUtilities.TestUtilities;

[assembly: CaptureConsole]
