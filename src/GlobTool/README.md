# vm2.GlobTool — Cross-Platform Glob Pattern Matching Tool

[![CI](https://github.com/vmelamed/vm2.Glob/actions/workflows/CI.yaml/badge.svg?branch=main)](https://github.com/vmelamed/vm2.Glob/actions/workflows/CI.yaml)
[![codecov](https://codecov.io/gh/vmelamed/vm2.Glob/branch/main/graph/badge.svg?branch=main)](https://codecov.io/gh/vmelamed/vm2.Glob)
[![Release](https://github.com/vmelamed/vm2.Glob/actions/workflows/Release.yaml/badge.svg?branch=main)](https://github.com/vmelamed/vm2.Glob/actions/workflows/Release.yaml)

[![NuGet Version](https://img.shields.io/nuget/v/vm2.GlobTool)](https://www.nuget.org/packages/vm2.GlobTool/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/vm2.GlobTool.svg)](https://www.nuget.org/packages/vm2.GlobTool/)
[![GitHub License](https://img.shields.io/github/license/vmelamed/vm2.Glob)](https://github.com/vmelamed/vm2.Glob/blob/main/LICENSE)

<!-- TOC tocDepth:2..5 chapterDepth:2..6 -->

- [Installation](#installation)
- [Quick Start](#quick-start)
- [What Are Glob Patterns?](#what-are-glob-patterns)
- [Command Line Options](#command-line-options)
- [Glob Pattern Syntax](#glob-pattern-syntax)
- [Examples](#examples)
  - [Basic Usage](#basic-usage)
  - [Directory-Specific Searches](#directory-specific-searches)
  - [Object Type Selection](#object-type-selection)
  - [Case Sensitivity](#case-sensitivity)
  - [Advanced Patterns](#advanced-patterns)
  - [Deduplication](#deduplication)
  - [Including Hidden Files](#including-hidden-files)
- [Output Format](#output-format)
- [Environment Variable Support](#environment-variable-support)
  - [Windows](#windows)
  - [Unix, Linux, macOS](#unix-linux-macos)
- [Real-World Use Cases](#real-world-use-cases)
  - [Development Workflows](#development-workflows)
  - [CI/CD Integration](#cicd-integration)
    - [GitHub Actions](#github-actions)
    - [Azure Pipelines](#azure-pipelines)
  - [Code Analysis](#code-analysis)
  - [Project Maintenance](#project-maintenance)
- [Performance Tips](#performance-tips)
- [Comparison with Alternatives](#comparison-with-alternatives)
- [Troubleshooting](#troubleshooting)
  - [Pattern Not Matching](#pattern-not-matching)
  - [Permission Errors](#permission-errors)
  - [No Results](#no-results)
- [Related Packages](#related-packages)
- [License](#license)

<!-- /TOC -->

A fast, cross-platform CLI tool for finding files and directories using
[glob patterns](https://www.man7.org/linux/man-pages/man7/glob.7.html). Built on the
[vm2.Glob.Api](https://www.nuget.org/packages/vm2.Glob.Api/) library, it brings the familiar wildcard syntax of Unix shells to
every operating system — with environment variable expansion, configurable case sensitivity, and clean, pipe-friendly output.

## Installation

```bash
dotnet tool install -g vm2.GlobTool
```

## Quick Start

```bash
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
```

## What Are Glob Patterns?

Glob patterns are a concise wildcard notation for matching file and directory paths. They originated in early Unix shells and
are the same syntax used by `.gitignore`, build tools, and many editors. The `glob` tool lets you use these patterns directly
from the command line, on any operating system.

## Command Line Options

```text
glob <pattern> [options]

Arguments:
  glob                   Glob pattern (e.g., '**/*.txt')

Options:
  -d, --start-from       Start directory (default: current directory)
  -o, --search-objects   What to find: files|f, directories|d, both|b (default: both)
  -c, --case             Case sensitivity: sensitive|s, insensitive|i, platform|p (default: platform)
  -x, --distinct         Remove duplicate results (default: false)
  -a, --show-hidden      Include hidden/system files (default: false)
  --help                 Show help and usage information
  --version              Show version information
```

## Glob Pattern Syntax

| Pattern     | Meaning                                            |
|-------------|----------------------------------------------------|
| `*`         | Any sequence of characters (except path separator) |
| `?`         | Any single character                               |
| `[abc]`     | Any character in set (a, b, or c)                  |
| `[a-z]`     | Any character in range                             |
| `[!abc]`    | Any character NOT in set                           |
| `**`        | Zero or more directory levels (globstar)           |
| `[:alpha:]` | Named character class                              |

## Examples

### Basic Usage

```bash
# Find all C# files
glob "**/*.cs"

# Find test files
glob "**/*Tests.cs"

# Find JSON config files in the current directory
glob "*.json"
```

### Directory-Specific Searches

```bash
# Search in a specific directory
glob "**/*.txt" -d ~/documents

# Search from home directory
glob "**/*.log" -d ~

# Search with an absolute path
glob "**/*.md" -d /usr/share/doc
```

### Object Type Selection

```bash
# Find only files
glob "**/*.dll" -o files

# Find only directories
glob "**" -o directories

# Find both (default)
glob "src/**" -o both
```

### Case Sensitivity

```bash
# Case-sensitive (exact match required)
glob "[A-Z]*.cs" -c sensitive

# Case-insensitive (README.md matches readme.md)
glob "readme.md" -c insensitive

# Platform default (insensitive on Windows, sensitive on Unix)
glob "*.TXT" -c platform
```

### Advanced Patterns

```bash
# Character classes
glob "**/*[0-9].log"           # Files ending with a digit
glob "**/[a-z]*.cs"            # Files starting with a lowercase letter

# Named character classes
glob "**/*[[:digit:]].txt"     # Files ending with a digit
glob "**/*[[:alpha:]]*.cs"     # Files containing a letter

# Negation in character classes
glob "**/[!.]*.json"           # JSON files whose name does not start with a dot

# Environment variables (expanded before matching)
glob "$HOME/documents/**/*.pdf"              # Unix
glob "%USERPROFILE%\documents\**\*.pdf"      # Windows
glob "~/documents/**/*.pdf"                  # ~ expands to $HOME on Unix
```

### Deduplication

Patterns with multiple `**` segments (e.g., `**/docs/**/*.md`) can enumerate the same path more than once. Use `-x` to
deduplicate:

```bash
# Without --distinct (may show duplicates)
glob "**/docs/**/*.md"

# With --distinct (removes duplicates)
glob "**/docs/**/*.md" -x
```

### Including Hidden Files

By default, hidden and system files are excluded. Use `-a` to include them:

```bash
# Exclude hidden/system files (default)
glob "**/*"

# Include hidden/system files (e.g., .gitignore, .env)
glob "**/*" -a
```

## Output Format

Each matched path is printed on a separate line as an absolute path. Directory paths end with the platform's directory separator
(`/` on Unix, `\` on Windows). The output contains no extra formatting or colors, making it ideal for piping into other tools.

Example output:

```text
/home/user/projects/MyApp/src/Program.cs
/home/user/projects/MyApp/src/Models/User.cs
/home/user/projects/MyApp/test/ProgramTests.cs
```

## Environment Variable Support

The tool expands environment variables in the pattern before matching:

### Windows

```bash
glob "%APPDATA%\**\*.json"
glob "%USERPROFILE%\Documents\**\*.txt"
```

### Unix, Linux, macOS

```bash
glob "$HOME/documents/**/*.pdf"
glob "~/projects/**/*.cs"        # ~ expands to $HOME
glob "$XDG_CONFIG_HOME/**/*.conf"
```

## Real-World Use Cases

### Development Workflows

```bash
# Find all unit test files
glob "**/test/**/*Tests.cs"

# Find configuration files
glob "**/appsettings*.json"

# Find source files in a specific subtree
glob "src/**/*.cs"
```

### CI/CD Integration

#### GitHub Actions

```yaml
- name: Find test assemblies
  run: |
    TEST_DLLS=$(glob "**/*Tests.dll" -d ./artifacts/bin)
    dotnet test $TEST_DLLS
```

#### Azure Pipelines

```yaml
- script: |
    FILES=$(glob "**/*.csproj")
    echo "##vso[task.setvariable variable=ProjectFiles]$FILES"
```

### Code Analysis

```bash
# Find public interfaces
glob "src/**/I*.cs" | xargs grep "public interface"

# Find deprecated code
glob "**/*.cs" | xargs grep -l "Obsolete"

# Count lines of code
glob "src/**/*.cs" | xargs wc -l
```

### Project Maintenance

```bash
# Find package references
glob "**/*.csproj" | xargs grep PackageReference

# Find large files
glob "**/*" | xargs du -h | sort -rh | head -20

# Find old log files
glob "**/*.log" -d /var/log
```

## Performance Tips

1. **Be specific** — `src/**/*.cs` is faster than `**/*.cs`.
2. **Select the right object type** — use `-o files` when you only need files.
3. **Minimize globstars** — `docs/**/*.md` is faster than `**/docs/**/*.md`.
4. **Use `-x` only when needed** — deduplication has a memory cost proportional to result count.

## Comparison with Alternatives

| Feature          | `glob`        | `find` (Unix) | `Get-ChildItem` (PS) | `fd`      |
|:-----------------|:--------------|:--------------|:---------------------|:----------|
| Cross-platform   | ✅            | ❌            | ❌                   | ✅        |
| Glob syntax      | ✅ Native     | ❌ Regex      | ❌ Complex           | ✅        |
| .NET integration | ✅            | ❌            | ⚠️                   | ❌        |
| Install          | `dotnet tool` | Pre-installed | Pre-installed        | Cargo     |
| Environment vars | ✅            | ❌            | ✅                   | ❌        |

## Troubleshooting

### Pattern Not Matching

Always quote the pattern to prevent the shell from expanding wildcards before the tool sees them:

```bash
glob "**/*.cs"     # ✅ Correct — shell passes the literal pattern
glob **/*.cs       # ❌ Wrong — shell expands before the tool runs
```

### Permission Errors

```bash
# Use elevated permissions (Windows)
glob "C:\Windows\System32\**\*.dll" -a

# Use sudo (Unix)
sudo glob "/root/**/*"
```

### No Results

```bash
# Verify the start directory exists
glob "**/*.cs" -d ~/nonexistent  # Error if path does not exist

# Check case sensitivity
glob "README.md" -c sensitive    # Won't match readme.md
glob "README.md" -c insensitive  # Matches readme.md
```

## Related Packages

- **[vm2.Glob.Api](https://www.nuget.org/packages/vm2.Glob.Api/)** — Glob pattern matching library for .NET applications
- **[POSIX.2 Glob Specification](https://www.man7.org/linux/man-pages/man7/glob.7.html)** — The underlying specification

## License

MIT — See [LICENSE](../../LICENSE)
