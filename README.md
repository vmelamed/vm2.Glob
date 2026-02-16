# vm2.Glob — Cross-Platform Glob Pattern Matching for .NET

[![CI](https://github.com/vmelamed/vm2.Glob/actions/workflows/CI.yaml/badge.svg?branch=main)](https://github.com/vmelamed/vm2.Glob/actions/workflows/CI.yaml)
[![codecov](https://codecov.io/gh/vmelamed/vm2.Glob/branch/main/graph/badge.svg?branch=main)](https://codecov.io/gh/vmelamed/vm2.Glob)
[![Release](https://github.com/vmelamed/vm2.Glob/actions/workflows/Release.yaml/badge.svg?branch=main)](https://github.com/vmelamed/vm2.Glob/actions/workflows/Release.yaml)

[![NuGet Version](https://img.shields.io/nuget/v/vm2.Glob.Api)](https://www.nuget.org/packages/vm2.Glob.Api/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/vm2.Glob.Api.svg)](https://www.nuget.org/packages/vm2.Glob.Api/)
[![GitHub License](https://img.shields.io/github/license/vmelamed/vm2.Glob)](https://github.com/vmelamed/vm2.Glob/blob/main/LICENSE)

<!-- TOC tocDepth:2..4 chapterDepth:2..6 -->

- [Overview](#overview)
  - [Features](#features)
- [Prerequisites](#prerequisites)
- [Install the Package (NuGet)](#install-the-package-nuget)
- [Quick Start](#quick-start)
- [Glob Pattern Syntax](#glob-pattern-syntax)
- [Get the Code](#get-the-code)
- [Build from the Source Code](#build-from-the-source-code)
- [Tests](#tests)
- [Benchmark Tests](#benchmark-tests)
- [Usage](#usage)
  - [Basic Enumeration](#basic-enumeration)
  - [Using the Fluent Builder](#using-the-fluent-builder)
  - [Dependency Injection](#dependency-injection)
  - [Advanced Configuration](#advanced-configuration)
  - [File System Access Control](#file-system-access-control)
    - [Include Hidden and System Files](#include-hidden-and-system-files)
    - [Skip Only Specific Attributes](#skip-only-specific-attributes)
    - [Handle Access-Denied Scenarios](#handle-access-denied-scenarios)
    - [Include Special Directory Entries](#include-special-directory-entries)
- [Configuration Options](#configuration-options)
  - [Object Type Selection](#object-type-selection)
  - [Case Sensitivity](#case-sensitivity)
  - [Traversal Order](#traversal-order)
  - [Deduplication](#deduplication)
- [Real-World Examples](#real-world-examples)
  - [Find Source Files, Excluding Build Output](#find-source-files-excluding-build-output)
  - [Find Test Assemblies](#find-test-assemblies)
  - [Clean Up Old Log Files](#clean-up-old-log-files)
  - [Load Configuration Files](#load-configuration-files)
- [Testing with IFileSystem](#testing-with-ifilesystem)
- [Performance](#performance)
  - [Best Practices](#best-practices)
  - [Memory Usage](#memory-usage)
  - [Benchmarks](#benchmarks)
- [API Reference](#api-reference)
  - [GlobEnumerator Class](#globenumerator-class)
    - [Constructor](#constructor)
    - [Properties](#properties)
    - [Methods](#methods)
  - [GlobEnumeratorBuilder Class](#globenumeratorbuilder-class)
  - [Extension Methods (Dependency Injection)](#extension-methods-dependency-injection)
- [Feature Requests & Roadmap](#feature-requests--roadmap)
  - [Pattern Extensions](#pattern-extensions)
  - [Tool Enhancements](#tool-enhancements)
- [Related Packages](#related-packages)
- [License](#license)
- [Version History](#version-history)

<!-- /TOC -->

## Overview

Glob patterns provide a concise, human-readable syntax for matching file and directory paths — the same wildcard notation used
by Unix shells, `.gitignore` files, and build systems. This repository provides two .NET packages for working with glob
patterns:

- **[vm2.Glob.Api](https://www.nuget.org/packages/vm2.Glob.Api/)** — A high-performance library for embedding glob-based file
  enumeration in .NET applications.
- **[vm2.GlobTool](src/GlobTool/README.md)** — A cross-platform command-line tool for finding files and directories from the
  terminal.

Both implement the [POSIX.2 glob specification](https://www.man7.org/linux/man-pages/man7/glob.7.html) with extensions for
Windows and Unix-like systems, including environment variable expansion and platform-aware case sensitivity.

### Features

- ✅ **[POSIX.2 glob specification](https://www.man7.org/linux/man-pages/man7/glob.7.html)** compliant with Windows extensions
- ✅ **Cross-platform** — identical behavior on Windows, Linux, and macOS
- ✅ **Environment variables** — automatic expansion of `$HOME`, `%USERPROFILE%`, and `~`
- ✅ **Flexible API** — fluent builder pattern for easy configuration
- ✅ **High performance** — optimized enumeration with minimal allocations
- ✅ **Lazy evaluation** — `IEnumerable`-based streaming of results
- ✅ **Testable** — `IFileSystem` abstraction for unit testing without touching the disk
- ✅ **Multiple traversal modes** — depth-first or breadth-first
- ✅ **Deduplication** — optional removal of duplicate results from multi-globstar patterns

## Prerequisites

- .NET 10.0 or later

## Install the Package (NuGet)

- Using the dotnet CLI:

  ```bash
  dotnet add package vm2.Glob.Api
  ```

- From Visual Studio **Package Manager Console**:

  ```powershell
  Install-Package vm2.Glob.Api
  ```

For the companion command-line tool, see [vm2.GlobTool](src/GlobTool/README.md).

## Quick Start

```csharp
using vm2.Glob.Api;

var enumerator = new GlobEnumerator
{
    Glob          = "**/*.cs",
    FromDirectory = "./src",
};

foreach (var path in enumerator.Enumerate())
    Console.WriteLine(path);
```

## Glob Pattern Syntax

| Pattern     | Meaning                                                  | Example                           |
|-------------|----------------------------------------------------------|-----------------------------------|
| `*`         | Any sequence of characters (except path separator)       | `*.txt` matches `file.txt`        |
| `?`         | Any single character                                     | `file?.txt` matches `file1.txt`   |
| `[abc]`     | Any character in set                                     | `[abc].txt` matches `a.txt`       |
| `[a-z]`     | Any character in range                                   | `[0-9].txt` matches `5.txt`       |
| `[!abc]`    | Any character NOT in set                                 | `[!.]*.txt` excludes hidden files |
| `**`        | Zero or more directory levels (globstar)                 | `**/test/**/*.cs` — recursive     |
| `[:class:]` | Named character class (alpha, digit, lower, upper, etc.) | `[[:digit:]]*.log`                |

## Get the Code

Clone the [GitHub repository](https://github.com/vmelamed/vm2.Glob). The library source is in the `src/Glob.Api` directory.

```bash
git clone https://github.com/vmelamed/vm2.Glob.git
cd vm2.Glob
```

## Build from the Source Code

- Command line:

  ```bash
  dotnet build
  ```

- Visual Studio / VS Code:
  - Open the solution and choose **Build Solution** (or **Rebuild** as needed).

## Tests

The test projects are in the `test` directory. They use MTP (Microsoft Testing Platform) with xUnit. Tests are buildable and
runnable from the command line and from Visual Studio Code across operating systems.

- Command line:

  ```bash
  dotnet test
  ```

- The tests can also be run standalone after building:

  ```bash
  dotnet build
  test/Glob.Api.Tests/bin/Debug/net10.0/Glob.Api.Tests
  ```

## Benchmark Tests

The benchmark project is in the `benchmarks/Glob.Api.Benchmarks` directory. It uses BenchmarkDotNet.

- Command line:

  ```bash
  dotnet run --project benchmarks/Glob.Api.Benchmarks/Glob.Api.Benchmarks.csproj -c Release
  ```

- Standalone after building:

  ```bash
  dotnet build -c Release benchmarks/Glob.Api.Benchmarks/Glob.Api.Benchmarks.csproj
  benchmarks/Glob.Api.Benchmarks/bin/Release/net10.0/Glob.Api.Benchmarks
  ```

## Usage

### Basic Enumeration

Create a `GlobEnumerator`, set the pattern and starting directory, then call `Enumerate()`:

```csharp
var enumerator = new GlobEnumerator
{
    Glob          = "**/*.cs",
    FromDirectory = "./src",
};

foreach (var file in enumerator.Enumerate())
    Console.WriteLine(file);
```

### Using the Fluent Builder

The `GlobEnumeratorBuilder` provides a fluent API for configuring and creating an enumerator in a single expression:

```csharp
var results = new GlobEnumeratorBuilder()
                    .WithGlob("**/*Tests.cs")
                    .FromDirectory("./test")
                    .SelectFiles()
                    .CaseSensitive()
                    .Build()
                    .Configure(new GlobEnumerator())
                    .Enumerate()
                    .ToList();
```

Or use `Create()` to get a pre-configured enumerator directly:

```csharp
var enumerator = new GlobEnumeratorBuilder()
                        .WithGlob("**/*.cs")
                        .FromDirectory("./src")
                        .SelectFiles()
                        .Build()
                        .Create();

foreach (var file in enumerator.Enumerate())
    Console.WriteLine(file);
```

### Dependency Injection

Register `GlobEnumerator` with your application's DI container using the provided extension methods:

```csharp
// In Startup.cs or Program.cs — register with default FileSystem
services.AddGlobEnumerator();

// In your service — resolve a configured enumerator
public class FileService(IServiceProvider sp)
{
    public IEnumerable<string> FindFiles(string pattern)
        => sp.GetGlobEnumerator(b => b.WithGlob(pattern).SelectFiles())
             .Enumerate();
}
```

### Advanced Configuration

The builder exposes the full range of enumerator options:

```csharp
var enumerator = new GlobEnumeratorBuilder()
                        .WithGlob("**/docs/**/*.md")
                        .FromDirectory("/usr/share")
                        .SelectFiles()
                        .CaseInsensitive()
                        .DepthFirst()
                        .Distinct()                   // remove duplicates from multi-globstar patterns
                        .Build()
                        .Configure(new GlobEnumerator());

foreach (var file in enumerator.Enumerate())
    ProcessFile(file);
```

### File System Access Control

#### Include Hidden and System Files

By default, the enumerator skips hidden and system files. On Unix-like systems this also excludes dotfiles
(e.g., `.gitignore`). Set `AttributesToSkip` to `None` to include everything:

```csharp
var enumerator = new GlobEnumerator
{
    Glob             = "**/*",
    FromDirectory    = "./src",
    AttributesToSkip = FileAttributes.None,   // include all files
};
```

#### Skip Only Specific Attributes

```csharp
// Skip only temporary files
enumerator.AttributesToSkip = FileAttributes.Temporary;

// Skip multiple attributes
enumerator.AttributesToSkip = FileAttributes.Hidden
                            | FileAttributes.System
                            | FileAttributes.Temporary;
```

#### Handle Access-Denied Scenarios

```csharp
// Throw on inaccessible files (strict mode)
enumerator.IgnoreInaccessible = false;

try
{
    foreach (var file in enumerator.Enumerate())
        ProcessFile(file);
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"Access denied: {ex.Message}");
}

// Skip inaccessible files silently (default, permissive mode)
enumerator.IgnoreInaccessible = true;
```

#### Include Special Directory Entries

```csharp
var enumerator = new GlobEnumerator
{
    Glob                     = "*",
    FromDirectory            = "./src",
    Enumerated               = Objects.Directories,
    ReturnSpecialDirectories = true,   // include "." and ".."
};
```

> **Note:** `ReturnSpecialDirectories` is rarely needed and defaults to `false` for cleaner results.

## Configuration Options

### Object Type Selection

```csharp
enumerator.Enumerated = Objects.Files;                // files only (default)
enumerator.Enumerated = Objects.Directories;          // directories only
enumerator.Enumerated = Objects.FilesAndDirectories;  // both
```

### Case Sensitivity

```csharp
enumerator.MatchCasing = MatchCasing.PlatformDefault;  // insensitive on Windows, sensitive on Unix (default)
enumerator.MatchCasing = MatchCasing.CaseSensitive;    // always case-sensitive
enumerator.MatchCasing = MatchCasing.CaseInsensitive;  // always case-insensitive
```

### Traversal Order

```csharp
enumerator.DepthFirst = false;  // breadth-first (default) — process siblings before children
enumerator.DepthFirst = true;   // depth-first — fully explore each subtree before moving on
```

### Deduplication

```csharp
enumerator.Distinct = false;    // allow duplicates (default, faster)
enumerator.Distinct = true;     // remove duplicates (uses a HashSet internally)
```

> **Note:** Deduplication is only necessary for patterns with multiple globstars (e.g., `**/docs/**/*.md`) that may enumerate
> the same path more than once.

## Real-World Examples

### Find Source Files, Excluding Build Output

```csharp
public IEnumerable<string> GetSourceFiles(string projectPath)
{
    var enumerator = new GlobEnumeratorBuilder()
                            .WithGlob("**/*.cs")
                            .FromDirectory(projectPath)
                            .SelectFiles()
                            .Build()
                            .Configure(new GlobEnumerator());

    return enumerator.Enumerate()
                     .Where(f => !f.Contains("/obj/") && !f.Contains("/bin/"));
}
```

### Find Test Assemblies

```csharp
public IEnumerable<string> FindTestAssemblies(string artifactsPath)
{
    var enumerator = new GlobEnumerator
    {
        Glob          = "**/*Tests.dll",
        FromDirectory = artifactsPath,
        Enumerated    = Objects.Files,
    };

    return enumerator.Enumerate();
}
```

### Clean Up Old Log Files

```csharp
public void CleanupLogs(string logDirectory, int daysOld)
{
    var cutoff = DateTime.Now.AddDays(-daysOld);

    var enumerator = new GlobEnumerator
    {
        Glob          = "**/*.log",
        FromDirectory = logDirectory,
    };

    foreach (var logFile in enumerator.Enumerate())
    {
        if (File.GetLastWriteTime(logFile) < cutoff)
            File.Delete(logFile);
    }
}
```

### Load Configuration Files

```csharp
public Dictionary<string, string> LoadConfigurations(string configPath)
{
    var enumerator = new GlobEnumeratorBuilder()
                            .WithGlob("**/appsettings*.json")
                            .FromDirectory(configPath)
                            .SelectFiles()
                            .CaseInsensitive()
                            .Build()
                            .Configure(new GlobEnumerator());

    return enumerator.Enumerate()
        .ToDictionary(
            f => Path.GetFileName(f),
            f => File.ReadAllText(f)
        );
}
```

## Testing with IFileSystem

The library provides an `IFileSystem` abstraction so that code depending on `GlobEnumerator` can be tested without touching the
file system. The repository includes a ready-made `FakeFileSystem` in the `test/Glob.Api.FakeFileSystem` project, but you can
also supply your own implementation:

```csharp
public class InMemoryFileSystem : IFileSystem
{
    // Implement: IsWindows, GetFullPath, GetCurrentDirectory,
    //            DirectoryExists, FileExists,
    //            EnumerateDirectories, EnumerateFiles
}

// Pass the custom file system to the enumerator
var enumerator = new GlobEnumerator(new InMemoryFileSystem())
{
    Glob          = "**/*.cs",
    FromDirectory = "/src",
};

var results = enumerator.Enumerate().ToList();
```

## Performance

### Best Practices

1. **Be specific with patterns** — `src/**/*.cs` is faster than `**/*.cs` because the search starts deeper in the tree.
2. **Use the appropriate object type** — `Objects.Files` avoids directory-enumeration overhead when you only need files.
3. **Minimize globstars** — each `**` increases traversal depth; avoid patterns like `**/a/**/b` when `a/**/b` suffices.
4. **Enable deduplication only when needed** — the internal `HashSet` has a memory cost proportional to the result count.
5. **Choose the right traversal order** — breadth-first works well for wide, shallow trees where matches are near the top;
   depth-first is better for deep hierarchies.

### Memory Usage

- **Lazy enumeration** — results are streamed via `IEnumerable`, not materialized into a list.
- **Minimal allocations** — uses `Span<T>` and `stackalloc` internally for pattern parsing and transformation.
- **Deduplication cost** — when `Distinct` is enabled, a `HashSet<string>` tracks every returned path.

### Benchmarks

Typical performance on standard hardware:

| Operation                   | Files | Time   | Allocations |
|-----------------------------|-------|-------:|-------------|
| Simple pattern (`*.cs`)     |   100 | ~1ms   | <1KB        |
| Recursive (`**/*.cs`)       | 1,000 | ~50ms  | ~50KB       |
| Complex (`**/test/**/*.cs`) | 1,000 | ~80ms  | ~80KB       |
| With distinct               | 1,000 | ~100ms | ~150KB      |

## API Reference

### GlobEnumerator Class

#### Constructor

```csharp
GlobEnumerator(IFileSystem? fileSystem = null, ILogger<GlobEnumerator>? logger = null)
```

Both parameters are optional. When `fileSystem` is `null`, the enumerator uses the real file system.

#### Properties

| Property                   | Type              | Default                            | Description                                                 |
|----------------------------|-------------------|------------------------------------|-------------------------------------------------------------|
| `Glob`                     | `string`          | `""` (treated as `"*"`)            | The glob pattern to match.                                  |
| `FromDirectory`            | `string`          | `"."` (current directory)          | Starting directory for enumeration.                         |
| `Enumerated`               | `Objects`         | `Files`                            | `Files`, `Directories`, or `FilesAndDirectories`.           |
| `MatchCasing`              | `MatchCasing`     | `PlatformDefault`                  | `PlatformDefault`, `CaseSensitive`, or `CaseInsensitive`.   |
| `DepthFirst`               | `bool`            | `false`                            | `true` for depth-first; `false` for breadth-first.          |
| `Distinct`                 | `bool`            | `false`                            | Remove duplicate paths from results.                        |
| `ReturnSpecialDirectories` | `bool`            | `false`                            | Include `"."` and `".."` entries.                           |
| `IgnoreInaccessible`       | `bool`            | `true`                             | Skip entries that throw access-denied exceptions.           |
| `AttributesToSkip`         | `FileAttributes`  | `Hidden \| System`                 | Skip entries with these file attributes.                    |

#### Methods

| Method          | Returns                | Description                          |
|-----------------|------------------------|--------------------------------------|
| `Enumerate()`   | `IEnumerable<string>`  | Execute the glob and stream matches. |

### GlobEnumeratorBuilder Class

All builder methods return the builder instance for method chaining.

| Method                                            | Description                                            |
|---------------------------------------------------|--------------------------------------------------------|
| `WithGlob(string pattern)`                        | Set the glob pattern.                                  |
| `FromDirectory(string path)`                      | Set the starting directory.                            |
| `SelectFiles()`                                   | Enumerate files only.                                  |
| `SelectDirectories()`                             | Enumerate directories only.                            |
| `SelectDirectoriesAndFiles()`                     | Enumerate both.                                        |
| `Select(Objects type)`                            | Set object type explicitly.                            |
| `CaseSensitive()`                                 | Case-sensitive matching.                               |
| `CaseInsensitive()`                               | Case-insensitive matching.                             |
| `PlatformSensitive()`                             | Platform-default case sensitivity.                     |
| `WithCaseSensitivity(MatchCasing casing)`         | Set case sensitivity explicitly.                       |
| `DepthFirst()`                                    | Depth-first traversal.                                 |
| `BreadthFirst()`                                  | Breadth-first traversal (default).                     |
| `TraverseDepthFirst(TraverseOrder order)`         | Set traversal order explicitly.                        |
| `Distinct()`                                      | Enable deduplication.                                  |
| `WithDistinct(bool distinct)`                     | Set deduplication explicitly.                          |
| `IncludeSpecialDirectories(bool include = true)`  | Include `"."` and `".."` entries.                      |
| `SkipInaccessible(bool skip = true)`              | Skip access-denied entries.                            |
| `SkipObjectsWithAttributes(FileAttributes attrs)` | Skip entries with specified attributes.                |
| `Build()`                                         | Finalize the builder (returns `this`).                 |
| `Create()`                                        | Build and return a new configured `GlobEnumerator`.    |
| `Configure(GlobEnumerator enumerator)`            | Apply settings to an existing `GlobEnumerator`.        |

### Extension Methods (Dependency Injection)

```csharp
// Register GlobEnumerator with default FileSystem
services.AddGlobEnumerator();

// Register with a builder configuration
services.AddGlobEnumerator(b => b.SelectFiles().CaseSensitive());

// Resolve a configured enumerator from the service provider
var enumerator = serviceProvider.GetGlobEnumerator(
    b => b.WithGlob("**/*.cs").FromDirectory("./src"));
```

## Feature Requests & Roadmap

Have a feature you'd like to see? Open an issue or upvote an existing request. The **Votes** column reflects community interest
and helps prioritize development.

### Pattern Extensions

| Votes | Feature                | Syntax                    | Description                                                                     | Status |
|------:|:-----------------------|:--------------------------|:--------------------------------------------------------------------------------|:------:|
|    10 | Brace expansion        | `{a,b,c}`                 | Expand comma-separated alternatives: `*.{cs,fs}` matches both `*.cs` and `*.fs` |   ❌   |
|     8 | Exclusion patterns     | `!pattern` or `--exclude` | Exclude paths matching a pattern, e.g. `**/*.cs` with `!**/obj/**`              |   ❌   |
|     6 | Multiple patterns      | repeated args or `-p`     | Accept several patterns in one invocation: `glob "**/*.cs" "**/*.fs"`           |   ❌   |
|     4 | Max depth limit        | `--max-depth N`           | Restrict how deep `**` can descend                                              |   ❌   |
|     0 | Backslash escaping     | `\*`, `\?`, `\[`          | Escape special characters with `\` instead of bracket notation `[*]`            |   ❌   |
|     0 | Numeric ranges         | `{1..10}`                 | Generate a sequence of numbers as part of brace expansion                       |   ❌   |
|     0 | Extglob — optional     | `?(pattern)`              | Match zero or one occurrence of the pattern                                     |   ❌   |
|     0 | Extglob — one-or-more  | `+(pattern)`              | Match one or more occurrences                                                   |   ❌   |
|     0 | Extglob — zero-or-more | `*(pattern)`              | Match zero or more occurrences                                                  |   ❌   |
|     0 | Extglob — exactly one  | `@(a\|b)`                 | Match exactly one of the pipe-delimited alternatives                            |   ❌   |
|     0 | Extglob — negation     | `!(pattern)`              | Match anything *except* the pattern                                             |   ❌   |
|     0 | Alternation            | `(a\|b)`                  | Inline alternatives without full brace expansion                                |   ❌   |

### Tool Enhancements

| Votes | Feature                | Syntax                    | Description                                                                     | Status |
|------:|:-----------------------|:--------------------------|:--------------------------------------------------------------------------------|:------:|
|     0 | Min depth limit        | `--min-depth N`           | Skip results shallower than N levels                                            |   ❌   |
|     0 | Dotglob mode           | `--dotglob`               | Let `*` and `**` match leading dots without including system files              |   ❌   |
|     0 | Follow symlinks        | `--follow-links`          | Follow symbolic links during traversal                                          |   ❌   |
|     0 | Null-delimited output  | `-0`, `--print0`          | Use `\0` as delimiter (safe for filenames with spaces)                          |   ❌   |
|     0 | Count-only mode        | `--count`                 | Print only the number of matches                                                |   ❌   |
|     0 | Regex fallback         | `r:pattern` prefix        | Allow a raw regex when glob syntax is insufficient                              |   ❌   |
|     0 | File metadata filters  | `--newer`, `--larger`     | Post-match filters on age, size, etc.                                           |   ❌   |

## Related Packages

- **[vm2.GlobTool](src/GlobTool/README.md)** — Cross-platform command-line tool for glob pattern matching
- **[POSIX.2 Glob Specification](https://www.man7.org/linux/man-pages/man7/glob.7.html)** — The Linux man-pages project
- **[Glob (programming) — Wikipedia](https://en.wikipedia.org/wiki/Glob_(programming))**

## License

MIT — See [LICENSE](LICENSE)

## Version History

See [CHANGELOG.md](CHANGELOG.md) for version history and release notes.
