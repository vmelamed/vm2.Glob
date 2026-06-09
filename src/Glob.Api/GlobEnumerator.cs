// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

namespace vm2.Glob.Api;

/// <summary>
/// Represents a glob pattern searcher.
/// </summary>
/// <threadsafety>
/// The <see cref="GlobEnumerator"/> class is not thread-safe. It is the responsibility of the caller to ensure that instances
/// of this class are not accessed concurrently from multiple threads.
/// </threadsafety>
public sealed partial class GlobEnumerator
{
    #region Fields and private properties
    RegexOptions _regexOptions = RegexOptions.IgnorePatternWhitespace
                                | RegexOptions.ExplicitCapture
                                | (OperatingSystem.RegexOptions);

    EnumerationOptions _options = new()
    {
        MatchCasing = MatchCasing.PlatformDefault, // see also _matchCasing property
        RecurseSubdirectories = false,                       // we control it ourselves
        MatchType = MatchType.Simple,            // don't touch it - this is bs
        ReturnSpecialDirectories = false,
        IgnoreInaccessible = true,
        AttributesToSkip = FileAttributes.Hidden | FileAttributes.System,
    };

    ILogger<GlobEnumerator>? _logger;
    IFileSystem _fileSystem;
    string _glob = "";
    string _fromDir = "";
    Deque<(string dir, Range patternComponentRange, bool recursively)> _deque = [];

    /// <summary>
    /// The string comparer depends on the MatchCasing
    /// </summary>
    StringComparison StringComparison { get; set; } = OperatingSystem.Comparison;

    /// <summary>
    /// The string comparer depends on the MatchCasing
    /// </summary>
    StringComparer StringComparer { get; set; } = OperatingSystem.Comparison is StringComparison.Ordinal ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Gets a regex object that matches the root of the file system in a path.
    /// </summary>
    /// <returns>Regex</returns>
    Regex FileSystemRoot { get; init; }
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets or sets the glob used to match file or directory names. Default - an empty string, which is equivalent to "*".
    /// </summary>
    public string Glob
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            field = value;
        }
    } = "";

    /// <summary>
    /// Gets or sets the directory dirPath from which the enumeration operation begins. Default - from the current working directory.
    /// </summary>
    public string FromDirectory
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            var fullPath = _fileSystem.GetFullPath(value);

            if (!_fileSystem.DirectoryExists(fullPath))
                throw new ArgumentException("The specified directory to enumerate from does not exist.", nameof(value));

            field = _fileSystem.IsWindows ? value.Replace(WinSepChar, SepChar) : value;
        }
    } = ".";

    /// <summary>
    /// Gets or sets the type of file system objects to search for - files, directories, or both.
    /// </summary>
    public Objects Enumerated { get; set; } = Objects.Files;

    /// <summary>
    /// Gets or sets a value indicating whether the enumeration should be performed in depth-first order vs breadth-first. The
    /// default is <c>false</c> - breadth-first.
    /// </summary>
    public bool DepthFirst { get; set; }

    /// <summary>
    /// Gets or sets the case sensitivity behavior for matching operations.
    /// </summary>
    public MatchCasing MatchCasing
    {
        get => _options.MatchCasing;
        set
        {
            switch (value)
            {
                case MatchCasing.PlatformDefault:
                    if (_fileSystem.IsWindows)
                        goto case MatchCasing.CaseInsensitive;
                    else
                        goto case MatchCasing.CaseSensitive;

                case MatchCasing.CaseSensitive:
                    _regexOptions &= ~RegexOptions.IgnoreCase;
                    StringComparison = StringComparison.Ordinal;
                    StringComparer = StringComparer.Ordinal;
                    break;

                case MatchCasing.CaseInsensitive:
                    _regexOptions |= RegexOptions.IgnoreCase;
                    StringComparison = StringComparison.OrdinalIgnoreCase;
                    StringComparer = StringComparer.OrdinalIgnoreCase;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), "Invalid MatchCasing value.");
            }
            _options.MatchCasing = value;
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the results should be distinct. When set to <c>true</c>, duplicate results will
    /// be removed from the final output.
    /// </summary>
    /// <remarks>
    /// Some globs may lead to repeating matches, e.g., /**/docs/**/*.txt, which may not be desirable. But also it comes with
    /// a price in memory, performance, and loss of lazy enumeration. Therefore use judiciously.
    /// </remarks>
    public bool Distinct { get; set; } = false;

    /// <summary>
    /// Indicates whether to return the special directory entries "." and "..". Default: <c>false</c>
    /// </summary>
    public bool ReturnSpecialDirectories
    {
        get => _options.ReturnSpecialDirectories;
        set => _options.ReturnSpecialDirectories = value;
    }

    /// <summary>
    /// Indicates whether to skip files or directories when access is denied (for example, when accessing them would result in
    /// <see cref="UnauthorizedAccessException"/> or <see cref="SecurityException"/>).
    /// Default: <c>true</c>
    /// </summary>
    public bool IgnoreInaccessible
    {
        get => _options.IgnoreInaccessible;
        set => _options.IgnoreInaccessible = value;
    }

    /// <summary>
    /// Indicates whether to skip files or directories with the specified attributes.
    /// Default: <c><see cref="FileAttributes.Hidden"/> | <see cref="FileAttributes.System"/></c>
    /// </summary>
    public FileAttributes AttributesToSkip
    {
        get => _options.AttributesToSkip;
        set => _options.AttributesToSkip = value;
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="GlobEnumerator"/> class with <see cref="FileSystem"/> as the file system
    /// and <see cref="ILogger{GlobEnumerator}"/> logger.
    /// </summary>
    public GlobEnumerator(IFileSystem? fileSystem = null, ILogger<GlobEnumerator>? logger = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _logger = logger;
        MatchCasing = MatchCasing.PlatformDefault;
        FileSystemRoot = _fileSystem.FileSystemRootRegex();
        IgnoreInaccessible = true;
        AttributesToSkip = FileAttributes.Hidden | FileAttributes.System;
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Searches for files or directories that match the specified pattern within the configured directory.
    /// </summary>
    /// <threadsafety>
    /// The enumeration returned by this method is not thread-safe. It is the responsibility of the caller to ensure that the
    /// enumeration is not accessed concurrently from multiple threads.
    /// </threadsafety>
    public IEnumerable<string> Enumerate()
    {
        if (Enumerated is Objects.Files
            && Glob is not ""
            && (Glob.Last() is ('/' or '\\') || EndsWithGlobstarRegex().IsMatch(Glob)))
            throw new ArgumentException("Pattern cannot end with '/', '\\', or '**' when searching for files.");

        (_glob, _fromDir) = NormalizeGlobAndStartDir();
        if (!_fileSystem.GlobRegex().IsMatch(_glob))
            throw new ArgumentException("Invalid pattern.");

        if (_logger?.IsEnabled(LogLevel.Trace) is true)
            _logger.LogTrace("""
                ================================
                Matching the pattern:       "{Pattern}" => "{NormalizedPattern}"
                Current directory:          "{CurrentDir}"
                Enumerate from directory:      "{FromDir}" => "{NormalizedFromDir}"
                Objects:                    "{Enumerated}"
                """,
                Glob, _glob,
                _fileSystem.GetCurrentDirectory(),
                FromDirectory, _fromDir,
                Enumerated);

        return Traverse();
    }
    #endregion

    #region Private methods
    bool IsLastComponent(Range range) => range.End.Value >= _glob.Length;

    // After the normalization, the glob pattern has this shape: <glob.comp.1st>/<glob.comp.2nd>/.../<glob.comp.last>
    // the respective ranges are like this:                      ^--range 1-----^^--range 2-----^^...^--range last---^

    int EndOfFirstComponent() => _glob.IndexOf(SepChar) is int nextEnd && nextEnd is >= 0
                                                ? nextEnd : _glob.Length;

    int EndOfNextComponent(Range range) => _glob.IndexOf(SepChar, range.End.Value + 1) is int nextEnd && nextEnd is >= 0
                                                ? nextEnd : _glob.Length;

    // first pattern globComponent always starts at 0 and ends at the first SepChar, or at the end of the pattern
    Range FirstComponent() => 0..EndOfFirstComponent();

    // the nextStart is after the separator of the current range,
    // the nextEnd is at the next SepChar after the current range, or at the end of the pattern:
    Range NextComponent(Range range) => IsLastComponent(range)
                                            ? _glob.Length.._glob.Length // no next globComponent
                                            : (range.End.Value + 1)..EndOfNextComponent(range);

    IEnumerable<string> Traverse()
    {
        // Track visited paths only when Distinct is enabled and pattern has multiple globstars
        HashSet<string>? visited = Distinct && GlobstarRegex().Count(_glob) > 1
                                        ? new(StringComparer)
                                        : null;

        bool NotVisited(string path) => visited is null || visited.Add(path);

#if DEBUG
        Debug.Assert(_deque.Count is 0, "The queue must be empty after the previous search!");
#else
        if (_deque?.Count is not 0)
            throw new InvalidOperationException("The internal queue is not empty. Are you trying to start a new search before finishing the previous one?"+
                                                "This is not allowed. Please create a new instance of GlobEnumerator for the new search.");
#endif


        _deque.Clear();                                   // just in case
        _deque.IsStack = DepthFirst;                      // honor the order of traversing
        _deque.Add((_fromDir, FirstComponent(), false));  // enqueue the first search and dive-into the enumeration

        while (_deque.TryGet(out var p))
        {
            var (dir, componentRange, recursively) = p;

            var isLast = IsLastComponent(componentRange);
            var component = _glob[componentRange];
            var (pattern, regex) = ComponentToPatternRegex(component);  // globComponent -> pattern (for .NET) and
                                                                        // regex to filter the names of the objects in dir
            var rex = regex is "" || regex == _fileSystem.NameSequence
                        ? null
                        : new Regex($"(^|/){regex}$", _regexOptions);

            Func<string, bool> lastComponentMatches = regex switch
            {
                "" => path => LastComponent(path).Equals(pattern.AsSpan(), StringComparison),
                _ when regex == _fileSystem.NameSequence => path => true,
                // compose regex filtering after the file system pattern (we already set or cleared the RegexOptions.IgnoreCase)
                _ => path => rex!.IsMatch(LastComponent(path)),
            };

            if (_logger?.IsEnabled(LogLevel.Trace) is true)
                _logger.LogTrace("""
                    --------------------------------
                    searching in:               "{Directory}" {Recursively}
                    glob component:             "{Component}" {IsLastComponent}
                        pattern:                    "{Pattern}"
                        match regex:                "{Regex}"
                    """,
                    dir, recursively ? "recursively" : "",
                    component, isLast ? "(the last)" : "",
                    pattern, regex);

            var nextComponentRange = NextComponent(componentRange);

            // handle the special patterns and combinations
            switch (component, isLast, recursively)
            {
                case (CurrentDir, _, _):
                    // search again in the current dir
                    _deque.Add((dir, nextComponentRange, recursively));
                    continue;

                case (ParentDir, _, _):
                    // searching in the parent dir
                    _deque.Add((_fileSystem.GetFullPath($"{dir}/.."), nextComponentRange, recursively));
                    continue;

                case (Globstar, _, _):
                    // search again in the current dir but recursively!
                    _deque.Add((dir, nextComponentRange, true));
                    continue;

                case (_, isLast: false, recursively: false):
                    // we need to enqueue all matching sub-dirs of the current dir, and search in there for the next component
                    foreach (var subDir in _fileSystem
                                                .EnumerateDirectories(dir, pattern, _options)
                                                .Where(subDir => lastComponentMatches(subDir)))
                        _deque.Add((subDir, nextComponentRange, false));
                    break;

                case (_, isLast: false, recursively: true):
                    // we need to enqueue all sub-dirs of the current dir,
                    foreach (var subDir in _fileSystem.EnumerateDirectories(dir, SequenceWildcard, _options))
                    {
                        // pass recursively to all lower components and also
                        _deque.Add((subDir, componentRange, true));
                        // if the current component matches, pass non-recursively to the next component
                        if (lastComponentMatches(subDir))
                            _deque.Add((subDir, nextComponentRange, false));
                    }
                    break;

                case (_, isLast: true, recursively: false):
                    // we are at the last globComponent, non-recursively -
                    // just report the matches in the current dir and move on
                    if (Enumerated.HasFlag(Objects.Directories))
                        foreach (var subDir in _fileSystem
                                                    .EnumerateDirectories(dir, pattern, _options)
                                                    .Where(subDir => lastComponentMatches(subDir)))
                            if (NotVisited(subDir))
                            {
                                if (_logger?.IsEnabled(LogLevel.Trace) is true)
                                    _logger.LogTrace("          dir:  {Directory}", subDir);
                                yield return subDir;
                            }

                    if (Enumerated.HasFlag(Objects.Files))
                        foreach (var file in _fileSystem
                                                    .EnumerateFiles(dir, pattern, _options)
                                                    .Where(file => lastComponentMatches(file)))
                            if (NotVisited(file))
                            {
                                if (_logger?.IsEnabled(LogLevel.Trace) is true)
                                    _logger.LogTrace("          file: {File}", file);
                                yield return file;
                            }
                    break;

                case (_, isLast: true, recursively: true):
                    foreach (var subDir in _fileSystem
                                                .EnumerateDirectories(dir, SequenceWildcard, _options))
                    {
                        // we need to continue the recursive search in the sub-dirs to find matching objects deeper in the tree
                        _deque.Add((subDir, componentRange, true));

                        // report matching directories in the current dir
                        if (Enumerated.HasFlag(Objects.Directories)
                            && lastComponentMatches(subDir)
                            && NotVisited(subDir))
                        {
                            if (_logger?.IsEnabled(LogLevel.Trace) is true)
                                _logger.LogTrace("          dir:  {Directory}", subDir);
                            yield return subDir;
                        }
                    }

                    // report matching files in the current dir
                    if (Enumerated.HasFlag(Objects.Files))
                        foreach (var file in _fileSystem
                                                .EnumerateFiles(dir, pattern, _options)
                                                .Where(file => lastComponentMatches(file)))
                            if (NotVisited(file))
                            {
                                if (_logger?.IsEnabled(LogLevel.Trace) is true)
                                    _logger.LogTrace("          file: {File}", file);
                                yield return file;
                            }
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the last component of a subDir - the name of the file or directory at the end of the subDir as they are returned by
    /// EnumerateFiles/EnumerateDirectories.
    /// </summary>
    /// <param name="path"></param>
    /// <returns>
    /// A span representing the last component of the specified <paramref name="path"/>.
    /// </returns>
    static ReadOnlySpan<char> LastComponent(string path)
    {
        var span = path.EndsWith(SepChar) ? path.AsSpan()[..^1] : path.AsSpan();

        if (span.Length is <= 0)
            return span;

        var lastSep = span.LastIndexOf(SepChar);

        return lastSep is >= 0 ? span[(lastSep + 1)..] : span;
    }
    #endregion
}
