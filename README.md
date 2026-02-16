# vm2.Glob - Cross-Platform Glob Pattern Matching

[![CI](https://github.com/vmelamed/vm2.Glob/actions/workflows/CI.yaml/badge.svg)](https://github.com/vmelamed/vm2.Glob/actions/workflows/CI.yaml)
[![codecov](https://codecov.io/gh/vmelamed/vm2.Glob/branch/main/graph/badge.svg)](https://codecov.io/gh/vmelamed/vm2.Glob)
[![Release](https://github.com/vmelamed/vm2.Glob/actions/workflows/Release.yaml/badge.svg)](https://github.com/vmelamed/vm2.Glob/actions/workflows/Release.yaml)
[![NuGet](https://img.shields.io/nuget/v/vm2.Glob.Api.svg)](https://www.nuget.org/packages/vm2.Glob.Api)
[![NuGet Downloads](https://img.shields.io/nuget/dt/vm2.Glob.Api.svg)](https://www.nuget.org/packages/vm2.Glob.Api/)
[![License](https://img.shields.io/github/license/vmelamed/vm2.Glob)](https://github.com/vmelamed/vm2.Glob/blob/main/LICENSE)

<!-- TOC tocDepth:2..4 chapterDepth:2..6 -->

- [vm2.Glob.Api - Cross-Platform Glob Pattern Matching API Library](#vm2globapi---cross-platform-glob-pattern-matching-api-library)
  - [Installation](#installation)
  - [Quick Start](#quick-start)
  - [Features](#features)
  - [Glob Pattern Syntax](#glob-pattern-syntax)
  - [Usage](#usage)
    - [Basic Enumeration](#basic-enumeration)
    - [Using Fluent Builder](#using-fluent-builder)
    - [Dependency Injection](#dependency-injection)
    - [Advanced Configuration Using GlobEnumeratorBuilder](#advanced-configuration-using-globenumeratorbuilder)
    - [File System Access Control](#file-system-access-control)
  - [Configuration Options](#configuration-options)
    - [Object Type Selection](#object-type-selection)
    - [Case Sensitivity](#case-sensitivity)
    - [Traversal Order](#traversal-order)
    - [Deduplication](#deduplication)
  - [Real-World Examples](#real-world-examples)
    - [Build Tool - Find Source Files](#build-tool---find-source-files)
    - [Test Runner - Find Test Assemblies](#test-runner---find-test-assemblies)
    - [File Cleanup - Find Old Log Files](#file-cleanup---find-old-log-files)
    - [Configuration Loader - Find Config Files](#configuration-loader---find-config-files)
    - [ASP.NET Core - Static File Discovery](#aspnet-core---static-file-discovery)
  - [Testing with IFileSystem](#testing-with-ifilesystem)
    - [Custom Test Implementation](#custom-test-implementation)
  - [Performance Considerations](#performance-considerations)
    - [Best Practices](#best-practices)
    - [Memory Usage](#memory-usage)
    - [Benchmarks](#benchmarks)
  - [API Reference](#api-reference)
    - [GlobEnumerator Class](#globenumerator-class)
    - [`GlobEnumeratorBuilder` Class](#globenumeratorbuilder-class)
    - [Extension Methods](#extension-methods)
    - [Development Setup and Build of the Library](#development-setup-and-build-of-the-library)
  - [References](#references)
- [vm2.GlobTool - Cross-Platform Glob Pattern Matching Tool](#vm2globtool---cross-platform-glob-pattern-matching-tool)
  - [Tool Installation](#tool-installation)
  - [Tool Quick Start](#tool-quick-start)
  - [Tool Features](#tool-features)
  - [Glob Pattern Syntax (the same as Glob.Api - see above)](#glob-pattern-syntax-the-same-as-globapi---see-above)
  - [Command Line Options](#command-line-options)
  - [Examples](#examples)
    - [Basic Usage](#basic-usage)
    - [Directory Specific](#directory-specific)
    - [Object Type Selection (Tool Option -o, --search-objects)](#object-type-selection-tool-option--o---search-objects)
    - [Case Sensitivity (Tool Option -c, --case)](#case-sensitivity-tool-option--c---case)
    - [Advanced Patterns](#advanced-patterns)
    - [Deduplication (Tool Option -x, --distinct)](#deduplication-tool-option--x---distinct)
    - [Including Hidden Files](#including-hidden-files)
  - [Real-World Use Cases](#real-world-use-cases)
    - [Development Workflows](#development-workflows)
    - [CI/CD Integration](#cicd-integration)
    - [Code Analysis](#code-analysis)
    - [Project Maintenance](#project-maintenance)
  - [Output Format](#output-format)
  - [Environment Variable Support](#environment-variable-support)
    - [Windows](#windows)
    - [Unix/Linux/macOS](#unixlinuxmacos)
  - [Performance Tips](#performance-tips)
  - [Comparison with Alternatives](#comparison-with-alternatives)
  - [Troubleshooting](#troubleshooting)
    - [Pattern Not Matching](#pattern-not-matching)
    - [Permission Errors](#permission-errors)
    - [No Results](#no-results)
  - [Library Integration](#library-integration)
- [License](#license)
- [Version History](#version-history)

<!-- /TOC -->

## vm2.Glob.Api - Cross-Platform Glob Pattern Matching API Library

A high-performance, cross-platform glob pattern matching library for .NET applications. Implements the
[POSIX.2 glob specification](https://www.man7.org/linux/man-pages/man7/glob.7.html) with extensions for Windows and Unix-like
systems.

### Installation

    dotnet add package vm2.Glob.Api

### Quick Start

    using vm2.Glob.Api;

    // Basic usage
    var enumerator = new GlobEnumerator();
    enumerator.Glob = "**/*.cs";
    enumerator.FromDirectory = "./src";

    foreach (var file in enumerator.Enumerate())
    {
        Console.WriteLine(file);
    }

### Features

- ✅ **[POSIX.2 glob specification](https://www.man7.org/linux/man-pages/man7/glob.7.html)** compliant with Windows extensions
- ✅ **Cross-platform** - Identical behavior on Windows, Linux, macOS, and BSD
- ✅ **High performance** - Optimized enumeration with minimal allocations
- ✅ **Flexible API** - Fluent builder pattern for easy configuration
- ✅ **Lazy evaluation** - IEnumerable-based streaming of results
- ✅ **Testable** - `IFileSystem` abstraction for unit testing
- ✅ **Environment variables** - Automatic expansion of path variables
- ✅ **Multiple traversal modes** - Depth-first or breadth-first
- ✅ **Deduplication** - Optional removal of duplicate results

### Glob Pattern Syntax

| Pattern     | Meaning                                                  | Example                           |
|-------------|----------------------------------------------------------|-----------------------------------|
| `*`         | Any sequence of characters (except path separator)       | `*.txt` matches `file.txt`        |
| `?`         | Any single character                                     | `file?.txt` matches `file1.txt`   |
| `[abc]`     | Any character in set                                     | `[abc].txt` matches `a.txt`       |
| `[a-z]`     | Any character in range                                   | `[0-9].txt` matches `5.txt`       |
| `[!abc]`    | Any character NOT in set                                 | `[!.]*.txt` excludes hidden files |
| `**`        | Zero or more directory levels (globstar)                 | `**/test/**/*.cs` recursive       |
| `[:class:]` | Named character class (alpha, digit, lower, upper, etc.) | `[[:digit:]]*.log`                |

### Usage

#### Basic Enumeration

    var enumerator = new GlobEnumerator
    {
        Glob = "**/*.cs",
        FromDirectory = "./src"
    };

    foreach (var file in enumerator.Enumerate())
    {
        Console.WriteLine(file);
    }

#### Using Fluent Builder

    var results = new GlobEnumeratorBuilder()
        .WithGlob("**/*Tests.cs")
        .FromDirectory("./test")
        .SelectFiles()
        .CaseSensitive()
        .Build()
        .Configure(new GlobEnumerator())
        .Enumerate()
        .ToList();

#### Dependency Injection

    // In Startup.cs or Program.cs
    services.AddGlobEnumerator();

    // In your service
    public class FileService
    {
        private readonly GlobEnumerator _globEnumerator;

        public FileService(GlobEnumerator globEnumerator)
        {
            _globEnumerator = globEnumerator;
        }

        public IEnumerable<string> FindFiles(string pattern)
        {
            _globEnumerator.Glob = pattern;
            return _globEnumerator.Enumerate();
        }
    }

#### Advanced Configuration Using GlobEnumeratorBuilder

    var enumerator = new GlobEnumeratorBuilder()
        .WithGlob("**/docs/**/*.md")
        .FromDirectory("/usr/share")
        .Select(Objects.Files)                    // Files only
        .WithCaseSensitivity(MatchCasing.CaseInsensitive)
        .TraverseDepthFirst(true)                 // Depth-first traversal
        .Distinct()                               // Remove duplicates
        .Build()
        .Configure(new GlobEnumerator());

    foreach (var file in enumerator.Enumerate())
    {
        ProcessFile(file);
    }

#### File System Access Control

##### Include Hidden and System Files

Also, useful on UNIX-like systems to include dotfiles (e.g., `.gitignore`).

    var enumerator = new GlobEnumerator
    {
        Glob = "**/*",
        FromDirectory = "./src",
        AttributesToSkip = FileAttributes.None  // Include all files
    };

    foreach (var file in enumerator.Enumerate())
    {
        Console.WriteLine(file);
    }

##### Skip Only Specific Attributes

    // Skip only temporary files
    enumerator.AttributesToSkip = FileAttributes.Temporary;

    // Skip multiple attributes
    enumerator.AttributesToSkip = FileAttributes.Hidden
                                | FileAttributes.System
                                | FileAttributes.Temporary;

##### Handle Access Denied Scenarios

    // Throw exceptions for inaccessible files (strict mode)
    enumerator.IgnoreInaccessible = false;

    try
    {
        foreach (var file in enumerator.Enumerate())
        {
            ProcessFile(file);
        }
    }
    catch (UnauthorizedAccessException ex)
    {
        Console.WriteLine($"Access denied: {ex.Message}");
    }

    // Skip inaccessible files silently (default, permissive mode)
    enumerator.IgnoreInaccessible = true;

    foreach (var file in enumerator.Enumerate())
    {
        // Will skip files/directories that can't be accessed
        ProcessFile(file);
    }

##### Include Special Directory Entries

    // Include "." and ".." in directory enumeration
    var enumerator = new GlobEnumerator
    {
        Glob = "*",
        FromDirectory = "./src",
        Enumerated = Objects.Directories,
        ReturnSpecialDirectories = true
    };

    foreach (var dir in enumerator.Enumerate())
    {
        // Will include ".", "..", and other directories
        Console.WriteLine(dir);
    }

**Note:** `ReturnSpecialDirectories` is rarely needed and defaults to `false` for cleaner results.

### Configuration Options

#### Object Type Selection

    // Find only files (default)
    enumerator.Enumerated = Objects.Files;

    // Find only directories
    enumerator.Enumerated = Objects.Directories;

    // Find both files and directories
    enumerator.Enumerated = Objects.FilesAndDirectories;

#### Case Sensitivity

    // Platform default (case-insensitive on Windows, sensitive on Unix)
    enumerator.MatchCasing = MatchCasing.PlatformDefault;

    // Always case-sensitive
    enumerator.MatchCasing = MatchCasing.CaseSensitive;

    // Always case-insensitive
    enumerator.MatchCasing = MatchCasing.CaseInsensitive;

#### Traversal Order

    // Breadth-first (default) - process all items at current level before descending
    enumerator.DepthFirst = false;

    // Depth-first - fully explore each subdirectory before siblings
    enumerator.DepthFirst = true;

#### Deduplication

    // Allow duplicate results (default, faster)
    enumerator.Distinct = false;

    // Remove duplicate results (uses more memory)
    enumerator.Distinct = true;

**Note:** Deduplication is only necessary for patterns with multiple globstars (e.g., `**/docs/**/*.md`) which may produce
duplicate results.

### Real-World Examples

#### Build Tool - Find Source Files

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

#### Test Runner - Find Test Assemblies

    public IEnumerable<string> FindTestAssemblies(string artifactsPath)
    {
        var enumerator = new GlobEnumerator
        {
            Glob = "**/*Tests.dll",
            FromDirectory = artifactsPath,
            Enumerated = Objects.Files
        };

        return enumerator.Enumerate();
    }

#### File Cleanup - Find Old Log Files

    public void CleanupLogs(string logDirectory, int daysOld)
    {
        var enumerator = new GlobEnumerator
        {
            Glob = "**/*.log",
            FromDirectory = logDirectory
        };

        var cutoffDate = DateTime.Now.AddDays(-daysOld);

        foreach (var logFile in enumerator.Enumerate())
        {
            if (File.GetLastWriteTime(logFile) < cutoffDate)
            {
                File.Delete(logFile);
            }
        }
    }

#### Configuration Loader - Find Config Files

    public Dictionary<string, string> LoadConfigurations(string configPath)
    {
        var enumerator = new GlobEnumeratorBuilder()
            .WithGlob("**/{appsettings,config}.json")
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

#### ASP.NET Core - Static File Discovery

    public void ConfigureStaticFiles(IApplicationBuilder app)
    {
        var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var enumerator = new GlobEnumerator
        {
            Glob = "**/*.{js,css,html}",
            FromDirectory = wwwroot,
            Enumerated = Objects.Files
        };

        foreach (var staticFile in enumerator.Enumerate())
        {
            // Process static files
            Console.WriteLine($"Found: {staticFile}");
        }
    }

### Testing with IFileSystem

The library provides `IFileSystem` abstraction for unit testing without file system access:

#### Custom Test Implementation

    public class InMemoryFileSystem : IFileSystem
    {
        private readonly Dictionary<string, List<string>> _structure;

        public bool IsWindows => false;

        public IEnumerable<string> EnumerateFiles(string path, string pattern, EnumerationOptions options)
        {
            return _structure.TryGetValue(path, out var files)
                ? files.Where(f => MatchesPattern(f, pattern))
                : Enumerable.Empty<string>();
        }

        // Implement other methods...
    }

    // In tests
    [Fact]
    public void GlobEnumerator_FindsExpectedFiles()
    {
        var fakeFs = new InMemoryFileSystem();
        fakeFs.AddFile("/src/Program.cs");
        fakeFs.AddFile("/src/Models/User.cs");

        var enumerator = new GlobEnumerator(fakeFs)
        {
            Glob = "**/*.cs",
            FromDirectory = "/src"
        };

        var results = enumerator.Enumerate().ToList();

        results.Should().HaveCount(2);
    }

### Performance Considerations

#### Best Practices

1. **Be specific with patterns** - `src/**/*.cs` is faster than `**/*.cs`
2. **Use appropriate object type** - `Objects.Files` skips directory enumeration overhead
3. **Avoid excessive globstars** - Each `**` increases traversal depth
4. **Use deduplication sparingly** - Only enable for multi-globstar patterns
5. **Prefer breadth-first for wide trees** - Better memory locality
6. **Prefer depth-first for deep trees** - Faster for deep hierarchies
7. **Prefer breadth-first for wide trees** - Faster for wide hierarchies or when the sought match is likely near the top and you
   intend to stop as early as possible.

#### Memory Usage

- **Lazy enumeration** - Results streamed, not materialized
- **Minimal allocations** - Uses `Span<T>` and `stackalloc` internally
- **Deduplication cost** - `HashSet<string>` for tracking seen paths

#### Benchmarks

Typical performance on standard hardware:

| Operation                   | Files | Time   | Allocations |
|-----------------------------|-------|-------:|-------------|
| Simple pattern (`*.cs`)     |   100 | ~1ms   | <1KB        |
| Recursive (`**/*.cs`)       | 1,000 | ~50ms  | ~50KB       |
| Complex (`**/test/**/*.cs`) | 1,000 | ~80ms  | ~80KB       |
| With distinct               | 1,000 | ~100ms | ~150KB      |

### API Reference

#### GlobEnumerator Class

##### Properties

###### Pattern and Directory

- `string Glob` - The glob pattern to match (default: `""` which is treated as `"*"`)
- `string FromDirectory` - Starting directory for enumeration (default: `"."` - current directory)

###### Object Selection

- `Objects Enumerated` - Type of objects to find: `Files`, `Directories`, or `FilesAndDirectories` (default: `Files`)

###### Matching Behavior

- `MatchCasing MatchCasing` - Case sensitivity mode: `PlatformDefault`, `CaseSensitive`, or `CaseInsensitive` (default: `PlatformDefault`)
- `bool DepthFirst` - Traversal order: `true` = depth-first, `false` = breadth-first (default: `false`)
- `bool Distinct` - Enable deduplication of results (default: `false`)

###### File System Behavior

- `bool ReturnSpecialDirectories` - Include special directory entries `"."` and `".."` in results (default: `false`)
- `bool IgnoreInaccessible` - Skip files/directories when access is denied (e.g., `UnauthorizedAccessException`, `SecurityException`) (default: `true`)
- `FileAttributes AttributesToSkip` - Skip files/directories with specified attributes (default: `FileAttributes.Hidden | FileAttributes.System`)

##### Methods

- `IEnumerable<string> Enumerate()` - Execute the glob pattern and return matching paths

##### Constructor

- `GlobEnumerator(IFileSystem? fileSystem = null, ILogger<GlobEnumerator>? logger = null)` - Create a new instance with optional custom file system and logger

#### `GlobEnumeratorBuilder` Class

##### Methods of the `GlobEnumeratorBuilder`

###### Pattern Configuration

- `WithGlob(string pattern)` - Set the glob pattern

###### Directory Configuration

- `FromDirectory(string path)` - Set starting directory

###### `GlobEnumeratorBuilder` Object Type Selection

- `SelectFiles()` - Find only files
- `SelectDirectories()` - Find only directories
- `SelectDirectoriesAndFiles()` - Find both files and directories
- `Select(Objects type)` - Set object type explicitly

###### `GlobEnumeratorBuilder` Case Sensitivity

- `CaseSensitive()` - Enable case-sensitive matching
- `CaseInsensitive()` - Enable case-insensitive matching
- `PlatformSensitive()` - Use platform default case sensitivity
- `WithCaseSensitivity(MatchCasing casing)` - Set case sensitivity explicitly

###### `GlobEnumeratorBuilder` Traversal Order

- `DepthFirst()` - Enable depth-first traversal
- `BreadthFirst()` - Enable breadth-first traversal (default)
- `TraverseDepthFirst(bool depthFirst)` - Set traversal order explicitly

###### `GlobEnumeratorBuilder` Result Filtering

- `Distinct()` - Enable deduplication
- `WithDistinct(bool distinct)` - Set deduplication explicitly

###### `GlobEnumeratorBuilder` File System Behavior

- `IncludeSpecialDirectories(bool include = true)` - Include `"."` and `".."` entries
- `SkipInaccessible(bool skip = true)` - Skip files/directories with access errors
- `SkipObjectsWithAttributes(FileAttributes attributes)` - Skip objects with specified attributes (e.g., `FileAttributes.Hidden`)

###### `GlobEnumeratorBuilder` Methods

- `Build()` - Build and return the builder (for method chaining)
- `Configure(GlobEnumerator enumerator)` - Apply configuration to an enumerator instance

#### Extension Methods

##### Dependency Injection with `GlobEnumeratorBuilder`

You can easily integrate `GlobEnumerator` with your .NET application's dependency injection (DI) system using the following extension methods:

    // Register with default FileSystem
    IServiceCollection.AddGlobEnumerator()

    // Register with custom FileSystem implementation
    IServiceCollection.AddGlobEnumerator<TFileSystem>()

    // Register with specific FileSystem instance
    IServiceCollection.AddGlobEnumerator(IFileSystem fileSystem)

##### Service Provider

    // Get configured enumerator from DI container
    IServiceProvider.GetGlobEnumerator(
        Func<GlobEnumeratorBuilder, GlobEnumeratorBuilder> configure)

#### Development Setup and Build of the Library

To set up the development environment and build the library, follow these steps:

- Clone the Repository

  ```sh
  git clone https://github.com/vmelamed/vm2.Glob
  cd vm2.Glob
  ```

- Build the Library

  ```sh
  dotnet build
  ```

- Run tests

  ```sh
  dotnet test
  ```

- Run benchmarks

  ```sh
  dotnet run -c Release --project benchmarks/Glob.Api.Benchmarks
  ```

### References

- [POSIX.2 Glob Specification](https://www.man7.org/linux/man-pages/man7/glob.7.html) - The Linux man-pages project
- [Glob (programming) - Wikipedia](https://en.wikipedia.org/wiki/Glob_(programming))
- [CommonMark Specification](https://spec.commonmark.org/) - Used for this documentation

## vm2.GlobTool - Cross-Platform Glob Pattern Matching Tool

A fast, intuitive CLI tool for finding files and directories using glob patterns.

### Tool Installation

    dotnet tool install -g vm2.GlobTool

### Tool Quick Start

    # Find all C# files recursively
    glob "**/*.cs"

    # Find files in a specific directory
    glob "**/*.txt" -d ~/documents

    # Find only directories
    glob "**" -o directories

    # Case-sensitive search
    glob "[A-Z]*.cs" -c sensitive

    # Remove duplicates from multi-globstar patterns
    glob "**/docs/**/*.md" -x

### Tool Features

- ✅ **[POSIX.2 glob specification](https://www.man7.org/linux/man-pages/man7/glob.7.html)** with Windows extensions
- ✅ **Cross-platform** - Windows, Linux, macOS
- ✅ **Fast** - Optimized enumeration algorithms
- ✅ **Flexible** - Files, directories, or both
- ✅ **Smart** - Environment variable expansion
- ✅ **Clean output** - Full absolute paths

### Glob Pattern Syntax (the same as Glob.Api - see above)

| Pattern     | Meaning                                            |
|-------------|----------------------------------------------------|
| `*`         | Any sequence of characters (except path separator) |
| `?`         | Any single character                               |
| `[abc]`     | Any character in set (a, b, or c)                  |
| `[a-z]`     | Any character in range                             |
| `[!abc]`    | Any character NOT in set                           |
| `**`        | Zero or more directory levels (globstar)           |
| `[:alpha:]` | Named character class                              |

### Command Line Options

    glob <pattern> [options]

    Arguments:
      glob                Glob pattern (e.g., '**/*.txt')

    Options:
      -d, --start-from       Start directory (default: current directory)
      -o, --search-objects   What to find: files|f, directories|d, both|b (default: both)
      -c, --case             Case sensitivity: sensitive|s, insensitive|i, platform|p (default: platform)
      -x, --distinct         Remove duplicate results (default: false)
      -a, --show-hidden      Include hidden/system files (default: false)
      --help                 Show help and usage information
      --version              Show version information

### Examples

#### Basic Usage

    # Find all C# files
    glob "**/*.cs"

    # Find test files
    glob "**/*Tests.cs"

    # Find JSON config files
    glob "*.json"

#### Directory Specific

    # Search in a specific directory
    glob "**/*.txt" -d ~/documents

    # Search from home directory
    glob "**/*.log" -d ~

    # Search with absolute path
    glob "**/*.md" -d /usr/share/doc

#### Object Type Selection (Tool Option -o, --search-objects)

    # Find only files
    glob "**/*.dll" -o files

    # Find only directories
    glob "**" -o directories

    # Find both (default)
    glob "src/**" -o both

#### Case Sensitivity (Tool Option -c, --case)

    # Case-sensitive (exact match required)
    glob "[A-Z]*.cs" -c sensitive

    # Case-insensitive (README.md matches readme.md)
    glob "readme.md" -c insensitive

    # Platform default (insensitive on Windows, sensitive on Unix)
    glob "*.TXT" -c platform

#### Advanced Patterns

    # Character classes
    glob "**/*[0-9].log"           # File names ending with digit
    glob "**/[a-z]*.cs"            # File names starting with lowercase

    # Named character classes
    glob "**/*[[:digit:]].txt"     # File names ending with a digit
    glob "**/[[:alpha:]]*.cs"      # File names starting with a letter

    # Negation
    glob "**/[!.]*.json"            # JSON files not starting with dot

    # Environment variables (expanded before matching)
    glob "$HOME/documents/**/*.pdf"              # Unix
    glob "%USERPROFILE%\documents\**\*.pdf"      # Windows
    glob "~/documents/**/*.pdf"                  # Unix (~ expands to $HOME)

#### Deduplication (Tool Option -x, --distinct)

    # Without distinct (may show duplicates)
    glob "**/docs/**/*.md"

    # With distinct (removes duplicates)
    glob "**/docs/**/*.md" -x

#### Including Hidden Files

    # Exclude hidden/system files (default)
    glob "**/*"

    # Include hidden/system files
    glob "**/*" -a

### Real-World Use Cases

#### Development Workflows

    # Find all unit test files
    glob "**/test/**/*Tests.cs"

    # Find configuration files
    glob "**/{appsettings,web.config}.json"

    # Find source files excluding tests
    glob "src/**/*.cs"

#### CI/CD Integration

##### GitHub Actions

    - name: Find test assemblies
      run: |
        TEST_DLLS=$(glob "**/*Tests.dll" -d ./artifacts/bin)
        dotnet test $TEST_DLLS

##### Azure Pipelines

    - script: |
        FILES=$(glob "**/*.csproj")
        echo "##vso[task.setvariable variable=ProjectFiles]$FILES"

#### Code Analysis

    # Find public interfaces
    glob "src/**/I*.cs" | xargs grep "public interface"

    # Find deprecated code
    glob "**/*.cs" | xargs grep -l "Obsolete"

    # Count lines of code
    glob "src/**/*.cs" | xargs wc -l

#### Project Maintenance

    # Find package references
    glob "**/*.csproj" | xargs grep PackageReference

    # Find large files
    glob "**/*" | xargs du -h | sort -rh | head -20

    # Find old log files
    glob "**/*.log" -d /var/log

### Output Format

Each matched path is printed on a separate line with:

- Absolute paths (full path from root)
- Directory paths end with `/` separator
- No extra formatting or colors (perfect for piping)

Example output:

    /home/user/projects/MyApp/src/Program.cs
    /home/user/projects/MyApp/src/Models/User.cs
    /home/user/projects/MyApp/test/ProgramTests.cs

### Environment Variable Support

The tool expands environment variables before pattern matching:

#### Windows

    glob "%APPDATA%\**\*.json"
    glob "%USERPROFILE%\Documents\**\*.txt"

#### Unix/Linux/macOS

    glob "$HOME/documents/**/*.pdf"
    glob "~/projects/**/*.cs"        # ~ expands to $HOME
    glob "$XDG_CONFIG_HOME/**/*.conf"

### Performance Tips

1. **Be specific** - `src/**/*.cs` is faster than `**/*.cs`
2. **Use `-o files`** if you only need files (skips directory enumeration)
3. **Avoid multiple globstars** unless necessary (`**/docs/**` is slower than `docs/**`)
4. **Use `-x` only when needed** (deduplication has memory cost)

### Comparison with Alternatives

| Feature          | `glob`         | `find` (Unix) | `Get-ChildItem` (PS) | `fd`      |
|:-----------------|:---------------|:--------------|:---------------------|:----------|
| Cross-platform   | ✅             | ❌           | ❌                   | ✅       |
| Glob syntax      | ✅ Native      | ❌ Regex     | ❌ Complex           | ✅       |
| .NET integration | ✅             | ❌           | ⚠️                   | ❌       |
| Install          | `dotnet tool`  | Pre-installed | Pre-installed        | Cargo     |
| Environment vars | ✅             | ❌           | ✅                   | ❌       |
| Speed            | Fast           | Very fast     | Slow                 | Very fast |

### Troubleshooting

#### Pattern Not Matching

    # Use quotes to prevent shell expansion
    glob "**/*.cs"     # Correct
    glob **/*.cs       # Wrong (shell expands before tool runs)

#### Permission Errors

    # Use elevated permissions (Windows)
    glob "C:\Windows\System32\**\*.dll" -a

    # Use sudo (Unix)
    sudo glob "/root/**/*"

#### No Results

    # Verify start directory exists
    glob "**/*.cs" -d ~/nonexistent  # Error

    # Check case sensitivity
    glob "README.md" -c sensitive    # Won't match readme.md
    glob "README.md" -c insensitive  # Matches readme.md

### Library Integration

This tool is built on the `vm2.Glob.Api` library. For programmatic access in .NET applications:

    dotnet add package vm2.Glob.Api

Example usage:

    using vm2.Glob.Api;

    var enumerator = new GlobEnumerator();
    enumerator.Glob = "**/*.cs";
    enumerator.FromDirectory = "/path/to/search";

    foreach (var file in enumerator.Enumerate())
    {
        Console.WriteLine(file);
    }

## License

MIT License - Copyright &copy; 2025 Val Melamed

See [LICENSE](../../LICENSE) for full text.

## Version History

See [CHANGELOG.md](../../CHANGELOG.md) for version history and release notes.
