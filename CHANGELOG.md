# Changelog

## v3.1.1-preview.1 - 2026-06-09

### Internal

- promote to stable v3.1.0 [skip ci]
- update changelog for v3.1.0 [skip ci]
- update dependencies

## v3.1.0 - 2026-06-09

See prereleases below.

## v3.1.0-preview.2 - 2026-06-09

### Added

- add null checks for path parameters in FileSystem and GlobEnumerator classes

### Fixed

- broken test messages

### Internal

- enhance error handling in SpanReader and add tests for negative size

## v3.1.0-preview.1 - 2026-06-09

### Added

- add max generation collection thresholds to CI environment variables [skip ci]

### Internal

- promote to stable v3.0.1 [skip ci]
- update changelog for v3.0.1 [skip ci]
- Bump the minor-and-patch group with 1 update
- update vm2.TestUtilities to version 2.1.0
- update vm2.TestUtilities to version 2.1.0

## v3.0.1 - 2026-06-05

See prereleases below.

## v3.0.1-preview.1 - 2026-06-05

### Fixed

- update .gitattributes for consistent line endings and add AOT guidelines to conventions
- update dependencies in lock files
- clarify comment in .gitattributes for text file normalization [skip ci]
- streamline the dev. environment for multi-OS/multi-IDE and for consistent configuration of AI [skip ci]
- correct AoT enablement in v3.0.0-preview.1 section
- update commit prefix for git-cliff to include 'tests' and adjust documentation
- remove trailing newline from file header template

### Internal

- diff-shared.sh
- change "test/" to "tests/"
- update dependencies
- update changelog for v3.0.0 [skip ci]

## v3.0.0 - 2026-05-29

See prereleases below.

## v3.0.0-preview.1 - 2026-05-28

### Fixed

- enable AoT, refactor Directory.Build.props
- update benchmark file references to use new naming conventions

### Internal

- package dependencies and update project references
- clean up usings
- **BREAKING:** renamed namespaces to follow the convention
- renamed test/ to tests/ for consistency

## v2.1.2-preview.1 - 2026-05-21

### Internal

- promote to stable v2.1.1 [skip ci]
- update changelog for v2.1.1 [skip ci]
- fix typos in conventions and CI warning message

## v2.1.1 - 2026-05-21

See prereleases below.

## v2.1.1-preview.1 - 2026-05-21

### Internal

- promote to stable v2.1.0 [skip ci]
- update changelog for v2.1.0 [skip ci]
- sync with diff-shared.sh
- update vm2.TestUtilities to version 1.5.1
- update vm2.TestUtilities to version 1.5.1 in package locks
- fix typo in conventions for merge or copy action description

## v2.1.0 - 2026-05-20

See prereleases below.

## v2.1.0-preview.1 - 2026-05-20

### Added

- add telemetry opt-out and first-time experience skip for .NET CLI [skip ci]
- add NSubstitute package references to test projects

### Fixed

- update test fixtures to handle broken C# Dev Kit and add IDisposable implementation

### Internal

- sync with diff-shared [skip ci]
- Bump the minor-and-patch group with 15 updates
- update project dependencies and configurations
- update package dependencies and add Copilot guidance
- update Copilot instructions and refactor test fixture setup

## v2.0.3-preview.1 - 2026-04-30

### Internal

- promote to stable v2.0.2 [skip ci]
- update changelog for v2.0.2 [skip ci]

## v2.0.2 - 2026-04-30

See prereleases below.

## v2.0.2-preview.4 - 2026-04-30

### Fixed

- commit prefix

### Internal

- Bump the minor-and-patch group with 1 update
- addressed copilot comments
- Bump the minor-and-patch group with 1 update
- dotnet restore --force-evaluate

### deps

- Bump the minor-and-patch group with 1 update

## v2.0.2-preview.3 - 2026-04-24

### Fixed

- adjust trim setting in changelog files for consistent formatting

### Internal

- clean-up changelog formatting [skip ci]

## v2.0.2-preview.2 - 2026-04-22

### Fixed

- correct invalid prerelease version headers in CHANGELOG

## v2.0.2-preview.1 - 2026-04-22

### Internal

- diff-shared

## v2.0.1 - 2026-04-14

See prereleases below.

## v2.0.1-preview.8 - 2026-04-14

### Internal

- update vm2.TestUtilities to 1.4.3; adjust changelog header formatting
- refresh lock files for vm2.TestUtilities 1.4.3

## v2.0.1-preview.7 - 2026-04-14

### Internal

- bump vm2.TestUtilities to 1.4.2 and align changelog parser
- update tag pattern in changelog configuration for semantic versioning
- update .gitattributes and add support for .slnx files; enhance Prerelease workflow with CHANGELOG reminder; modify changelog parsers for documentation messages; add PULL_REQUEST_TEMPLATE and .gitmessage for commit guidelines

### deps

- Bump the minor-and-patch group with 1 update

## v2.0.1-preview.6 - 2026-04-13

### Internal

- update changelog formatting and correct commit prefix for git-cliff

## v2.0.1-preview.5 - 2026-04-12

### Internal

- correct validation command in codecov.yaml and update source pattern in coverage.settings.xml
- collapse YAML block scalar in CI.yaml preprocessor description

## v2.0.1-preview.4 - 2026-04-11

### Internal

- update vm2.TestUtilities package version to 1.4.0 and refactor the test classes to inherit from TestBase

## v2.0.1-preview.3 - 2026-04-10

### Internal

- update changelog formatting and improve commit grouping in cliff.prerelease.toml

## v2.0.1-preview.2 - 2026-04-10

### Fixed

- update commit parsers in changelog configuration for improved categorization
- update .gitignore to include VS Code configuration files
- Update copyright year in all source files from 2025 to 2025-2026
- add missing global using directive for System.Text
- remove unused attributes from benchmark classes
- update workflow variables to provide default values for SAVE_PACKAGE_ARTIFACTS
- add missing attributes for JSON and Markdown export in BenchmarkBase
- update CI and Prerelease workflows to set default values for RESET_BENCHMARK_THRESHOLDS and VERBOSE variables; update package
  versions in lock files
- update DisableTestingPlatformServerCapability condition for Visual Studio builds
- update vm2.TestUtilities version to 1.3.1 in package lock files
- correct casing of 'items' in solution folder name
- curate CHANGELOG and fix git-cliff template for v2.x

### Internal

- update package versions in packages.lock.json and global.json to 10.0.5 [skip ci]
- diff-shared

## v2.0.1-preview.1 - 2026-03-25

### Internal

- Update changelog entries for v2.0.0-preview.1 and v2.0.0-preview.2

## v2.0.0 - 2026-03-24

See prereleases below.

## v2.0.0-preview.2 - 2026-03-24

### Internal

- Update GitHub Actions workflows to use environment variables directly

## v2.0.0-preview.1 - 2026-03-24

### Changed

- **BREAKING:** Removed `GlobEnumeratorBuilder.Build()` method
- **BREAKING:** `FileSystem` is now sealed
- Refactor `Deque` implementation to use `LinkedList<T>`
- Improve `GlobEnumerator` for better performance and clarity

### Fixed

- Fix insufficient buffer when replacement text is larger than destination span
- Add `--suppress-optimization-validator` option requiring high process priority

### Internal

- Update CI workflows, Dependabot config, and package dependencies
- Add CI gate job, benchmark threshold reset, and various workflow improvements

## v1.0.0 - 2026-02-18

Initial stable release.

## Usage Notes

> [!TIP] Be disciplined with your commit messages and let git-cliff do the work of updating this file.
>
> **Added:**
>
> - add new features here
> - commit prefix for git-cliff: `feat:`
>
> **Changed:**
>
> - add behavior changes here
> - commit prefix for git-cliff: `refactor:`
>
> **Fixed:**
>
> - add bug fixes here
> - commit prefix for git-cliff: `fix:`
>
> **Performance**
>
> - add performance improvements here
> - commit prefix for git-cliff: `perf:`
>
> **Removed**
>
> - add removed/obsolete items here
> - commit prefix for git-cliff: `revert:`
>
> **Security**
>
> - add security-related changes here
> - commit prefix for git-cliff: `security:`
>
> **Internal**
>
> - add internal changes here
> - commit prefix for git-cliff: `refactor:`, `doc:`, `docs:`, `style:`, `test:`, `tests:`, `chore:`, `ci:`, `build:`
>

## References

This format follows:

- [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
- [Semantic Versioning](https://semver.org/)
- Version numbers are produced by [MinVer](./ReleaseProcess.md) from Git tags.
