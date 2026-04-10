# Changelog

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
> - commit prefix for git-cliff: `refactor:`, `docs:`, `style:`, `test:`, `chore:`, `ci:`, `build:`
>

## References

This format follows:

- [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
- [Semantic Versioning](https://semver.org/)
- Version numbers are produced by [MinVer](./ReleaseProcess.md) from Git tags.
