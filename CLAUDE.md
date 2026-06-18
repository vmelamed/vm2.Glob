# vm2.Glob — Claude Context

@~/.claude/CLAUDE.md
@~/repos/vm2/CLAUDE.md
@.github/CONVENTIONS.md

## Package Identity

- Repo: <https://github.com/vmelamed/vm2.Glob>
- NuGet: <https://www.nuget.org/packages/vm2.Glob.Api/>, <https://www.nuget.org/packages/vm2.GlobTool/>
- Status: stable
- Target: .NET 10.0+

## What This Package Does

Glob patterns provide a concise, human-readable syntax for matching file and directory paths — the same wildcard notation used
by Unix shells, `.gitignore` files, and build systems. This repository provides two .NET packages for working with glob
patterns:

- **[vm2.Glob.Api](https://www.nuget.org/packages/vm2.Glob.Api/)** — A high-performance library for embedding glob-based file
  enumeration in .NET applications.
- **[vm2.GlobTool](src/GlobTool/README.md)** — A cross-platform command-line tool for finding files and directories from the
  terminal.

Both implement the [POSIX.2 glob specification](https://www.man7.org/linux/man-pages/man7/glob.7.html) with extensions for
Windows and Unix-like systems, including environment variable expansion and platform-aware case sensitivity.

## Features

See the [README](README.md) file for a detailed list of features and explanations.

## Common Local Commands

```bash
# Build
dotnet build vm2.Glob.slnx

# Run tests (xUnit v3, MTP v2 — each project is a compiled executable)
dotnet test --project tests/Glob.Api.FakeFileSystem.Tests/Glob.Api.FakeFileSystem.Tests.csproj
dotnet test --project tests/Glob.Api.Tests/Glob.Api.Tests.csproj

# Run test executables (xUnit v3, MTP v2 — each project is a compiled to an executable) on Linux:
tests/Glob.Api.FakeFileSystem.Tests/bin/Debug/net10.0/Glob.Api.FakeFileSystem.Tests
tests/Glob.Api.Tests/bin/Debug/net10.0/Glob.Api.Tests

# Run test executables (xUnit v3, MTP v2 — each project is a compiled to an executable) on Windows:

tests/Glob.Api.FakeFileSystem.Tests/bin/Debug/net10.0/Glob.Api.FakeFileSystem.Tests.exe
tests/Glob.Api.Tests/bin/Debug/net10.0/Glob.Api.Tests.exe

# Run a single test by method name (xUnit v3, MTP v2 filter syntax)
dotnet test --project tests/Glob.Api.Tests/Glob.Api.Tests.csproj --filter "MethodName_WhenCondition_ShouldOutcome"

# Pack NuGet package
dotnet pack vm2.Glob.slnx --configuration Release

# Run benchmarks (Release only)
dotnet run --project benchmarks/Glob.Api.Benchmarks/Glob.Api.Benchmarks.csproj --configuration Release -- --filter "*"
```

### Glob Pattern Syntax

| Pattern     | Meaning                                                  | Example                           |
|-------------|----------------------------------------------------------|-----------------------------------|
| `*`         | Any sequence of characters (except path separator)       | `*.txt` matches `file.txt`        |
| `?`         | Any single character                                     | `file?.txt` matches `file1.txt`   |
| `[abc]`     | Any character in set                                     | `[abc].txt` matches `a.txt`       |
| `[a-z]`     | Any character in range                                   | `[0-9].txt` matches `5.txt`       |
| `[!abc]`    | Any character NOT in set                                 | `[!.]*.txt` excludes hidden files |
| `**`        | Zero or more directory levels (globstar)                 | `**/tests/**/*.cs` — recursive    |
| `[:class:]` | Named character class (alpha, digit, lower, upper, etc.) | `[[:digit:]]*.log`                |

## Usage

### Basic Enumeration

Create a `GlobEnumerator`, set the pattern and starting directory, then call `Enumerate()`.

### Using the Fluent Builder

The `GlobEnumeratorBuilder` provides a fluent API for configuring and creating an enumerator in a single expression or use `Create()` to get a pre-configured enumerator directly.

The builder exposes the full range of enumerator options.

For detailed usage with examples see the [README](README.md) file.

### Dependency Injection

Register `GlobEnumerator` with your application's DI container using the provided extension methods.

## Testing with IFileSystem

The library uses an `IFileSystem` abstraction and implements a real file system in `src/Glob.Api/FileSystem.cs`  and a test, fake file system from `tests/Glob.Api.FakeFileSystem/Glob.Api.FakeFileSystem.csproj` so that code depending on `GlobEnumerator` can be tested without touching the real file system.

## Performance Characteristics

The performance of `GlobEnumerator` depends on several factors, including the complexity of the glob pattern, the number of files and directories being enumerated, and the configuration options such as `Distinct` and `DepthFirst`.
- Simple glob patterns (e.g., `*.cs`) are generally very fast.
- Patterns with multiple globstars (e.g., `**/*.cs`) may be slower, especially without deduplication.
- Enabling `Distinct` incurs a small overhead due to the use of a `HashSet` but prevents duplicate results.
- Depth-first traversal can be more memory-efficient for large directory trees compared to breadth-first traversal.
- Case-insensitive matching may be slightly slower on case-sensitive file systems due to additional string comparisons.
Overall, `GlobEnumerator` is designed to be efficient for common use cases, but performance should be evaluated for large-scale scenarios.

## Known Trade-offs and Design Notes

The design of `GlobEnumerator` depends on classic recursive traversal of directory trees. At each node of the tree, it evaluates the current directory against the glob pattern segment corresponding to the current level. At each level the glob segment is transformed to a regular expression which is then used to match the names of the children elements (sub-directories and files). This approach allows for correct matching but requires careful handling of recursion and regular expression transformation to maintain performance. One advantage to transforming to regular expressions is that it allows for future extending of the search patterns without changing the core traversal logic. However, this also means that complex glob patterns can incur the overhead of regular expression compilation and matching -- a classic trade-off between flexibility and performance.

In order to save on allocations, `GlobEnumerator` uses heavily `Span<char>` to work with substrings and segments of paths without creating new string instances. This reduces memory pressure and improves performance, especially when dealing with large directory trees and complex glob patterns.

The `vm2.Glob.Api.GlobEnumerator.Enumerate` method is the core method responsible for performing the actual enumeration of files and directories based on the glob pattern. It takes the starting directory and the glob pattern as inputs and lazily yields the matching file and directory paths according to the configured options such as `Distinct` and `DepthFirst`. This method leverages the recursive traversal logic and the regular expression transformation described above to efficiently match the glob pattern against the file system structure.

A couple of complementing patterns have emerged: `SpanReader` and `SpanWriter` that can be used in other projects.

## Active Work / Known Issues

- Parameterize the classes `SpanReader` and `SpanWriter` to make them more flexible and reusable in different contexts, potentially allowing them to work with different underlying data structures or span types rather than being tightly coupled to `Span<char>`.
- Extend the glob patterns to support more complex matching scenarios expected in more modern file systems and development environments, such as advanced wildcard usage, character classes, and nested patterns, while maintaining performance and efficiency.

## References

- **[vm2.GlobTool](src/GlobTool/README.md)** — Cross-platform command-line tool for glob pattern matching
- **[POSIX.2 Glob Specification](https://www.man7.org/linux/man-pages/man7/glob.7.html)** — The Linux man-pages project
- **[Glob (programming) — Wikipedia](https://en.wikipedia.org/wiki/Glob_(programming))**

## Prompting Notes

- The three projects in the `tests/` directory serve different purposes:
  - `Glob.Api.Tests` contains tests of Glob.Api
    - the `vm2.Tests.Glob.Api.GlobEnumeratorUnitTests` methods test against a fake file system where the file/directory structure is described in JSON or plain text files in the `tests/Glob.Api.Tests/FSFiles` folder.
    - the `vm2.Tests.Glob.Api.GlobEnumeratorIntegrationTests` methods test against a real file system. The file system is created temporarily from some of the files in the `tests/Glob.Api.Tests/FSFiles` folder and cleaned up afterward.
  - `tests/Glob.Api.FakeFileSystem/Glob.Api.FakeFileSystem.csproj` contains the project file for the `Glob.Api.FakeFileSystem` library, which provides a fake file system implementation for testing purposes
  - `tests/Glob.Api.FakeFileSystem.Tests/Glob.Api.FakeFileSystem.Tests.csproj` tests the `Glob.Api.FakeFileSystem` library, providing unit tests to verify the behavior and correctness of the fake file system implementation.
- The regex-per-segment approach means pattern changes can silently compile bad regexes — test edge cases.
- `GlobEnumerator` is mutable (init-style properties) — never treat it as immutable.
- `GlobEnumerator` is single-use. Once `Enumerate()` is called, `IsFrozen` is set to `true` permanently — the
  instance is dead after that, whether the enumeration completed or was abandoned. Any attempt to set a property
  or call `Enumerate()` again throws `InvalidOperationException`. Always create a new instance for a new
  enumeration; the builder and DI extension methods make this low-friction. Defense: allowing re-use would mean
  one object with two identities — the configured object and the running enumerator — with no safe moment to
  switch between them.

## Active Work / Roadmap

The README has a full prioritized roadmap table (Feature Requests & Roadmap section). Key pattern extensions planned,
in rough priority order:

| Syntax              | Feature                                                                                                |
| :------------------ | :------------------------------------------------------------------------------------------------------|
| `{a,b,c}`           | Brace expansion — `*.{cs,fs}` matches both `*.cs` and `*.fs`                                           |
| `{1..10}`           | Numeric ranges — generate a sequence as part of brace expansion                                        |
| `!pattern`          | Exclusion patterns — `**/*.cs` with `!**/obj/**`                                                       |
| `?(pat)`            | Extended glob — optional: zero or one occurrence                                                       |
| `+(pat)`            | Extended glob — one-or-more occurrences                                                                |
| `*(pat)`            | Extended glob — zero-or-more occurrences                                                               |
| `@(a\|b)`           | Extended glob — exactly one of the alternatives                                                        |
| `!(pat)`            | Extended glob — negation: match anything except the pattern                                            |
| `(a\|b)`            | Inline alternation without full brace expansion                                                        |
| `\*`, `\?`, `\[`    | Backslash escaping of special characters                                                               |

Also planned: `SpanReader` and `SpanWriter` generalization (currently `Span<char>`-only) for reuse in other packages.
