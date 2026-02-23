// SPDX-License-Identifier: MIT
// Copyright (c) 2025 Val Melamed

namespace vm2.Glob.Api;

/// <summary>
/// Represents a glob pattern searcher.
/// </summary>
public sealed partial class GlobEnumerator
{
    /// <summary>
    /// Escapes a span of characters for use in a regular expression glob.
    /// </summary>
    /// <remarks>
    /// Basically a rewrite of <see cref="Regex.Escape"/> for spans.<para/>
    /// </remarks>
    /// <param name="span">The span of characters to escape.</param>
    /// <param name="writer">The <see cref="SpanWriter"/> to write the escaped characters to. Most conservatively should have 2 x the length of <paramref name="span"/>.</param>
    static void RegexEscape(ReadOnlySpan<char> span, ref SpanWriter writer)
    {
        foreach (var ch in span)
        {
            if (RegexEscapable.Contains(ch))
                writer.Write('\\');
            writer.Write(ch);
        }
    }

    /// <summary>
    /// Normalizes the specified GlobRegex by converting separators, removing duplicate / and *, and determining the starting directory.
    /// Copies the normalized pattern into glob.
    /// Also sets the fromDir field based on the FromDirectory and the contents of the glob.
    /// </summary>
    /// <returns>A tuple containing the normalized pattern and the starting directory.</returns>
    (string normPattern, string fromDir) NormalizeGlobAndStartDir()
    {
        var m = FileSystemRoot.Match(Glob);

        // if it starts with a root or drive like `C:/` or just `/`
        string fromDir = "";
        int start;
        if (m.Success)
        {
            // then ignore fromDir and the current directory and start from the root of the file system
            fromDir = m.Value;
            start = m.Length; // skip the root part in the GlobRegex - it is reflected in fromDir
        }
        else
        {
            // get the full path of FromDirectory relative to the current dir
            fromDir = _fileSystem.GetFullPath(FromDirectory is "" ? CurrentDir : FromDirectory);
            start = 0;        // start from the beginning
        }
        var end = Glob.EndsWith(SepChar) ? Glob.Length - 1 : Glob.Length;  // ignore a trailing separator

        Span<char> patternSpan = stackalloc char[Glob.Length];

        Glob.AsSpan().CopyTo(patternSpan);

        char prev1 = '\0';  // the char previous to the current char
        char prev2 = '\0';  // the char before the previous char
        int i = start;

        for (var j = start; j < end; j++)
        {
            var ch = patternSpan[j];
            var c = ch is WinSepChar ? SepChar : ch;    // convert Windows separators to Unix-style

            if (c is SepChar && prev1 is SepChar)                           // Skip duplicate separators
                continue;                                                   // / <=> // <=> /// ...

            if (c is Asterisk && prev1 is Asterisk && prev2 is Asterisk)    // Skip asterisks after the globstar
                continue;                                                   // ** <=> *** <=> **** ...

            // slide by one
            prev2 = prev1;
            prev1 = c;
            patternSpan[i++] = c;
        }

        return (patternSpan[start..i].ToString(), fromDir);
    }

    /// <summary>
    /// Translates a POSIX glob to a .NET glob used in EnumerateDirectories and to a regex for final filtering.
    /// </summary>
    /// <param name="glob">The glob to translate.</param>
    /// <returns>A .NET path segment glob and the corresponding <see cref="Regex"/></returns>
    (string pattern, string regex) ComponentToPatternRegex(string glob)
    {
        // shortcut the easy cases
        var pr = glob switch
        {
            ""                      => (SequenceWildcard, _fileSystem.NameSequence),    // "*", ".*"
            SequenceWildcard        => (glob, _fileSystem.NameSequence),                // "*", ".*"
            CharacterWildcard       => (glob, _fileSystem.NameCharacter),               // "?", "."
            Globstar                => ("", ""),                                        // "", ""
            CurrentDir or ParentDir => (glob, ""),                                      // ".", "" or "..", ""
            _                       => default                                          // (null, null) <=> "I don't know yet"
        };

        if (pr is not (null, null))
            return pr;

        // we don't know yet: analyze the glob
        var (pattern, regex) = pr;
        var matches = GlobExpressionRegex().Matches(glob);

        if (matches.Count is 0)                                     // no wildcards
        {
            if (!glob.AsSpan().ContainsAny(RegexChars))
                return (glob, "");                                 // no need to escape

            var writer = new SpanWriter(stackalloc char[2 * glob.Length]);
            RegexEscape(glob.AsSpan(), ref writer);
            return (glob, writer.Chars.ToString());
        }

        // now the glob can be thought of as a sequence of non-matching and matching slices:
        // (<non-match><match>)*<non-match>
        // where each non-match element can be empty

        // span to go through the glob
        var globReader = new SpanReader(glob.AsSpan());
        var pool = ArrayPool<char>.Shared;
        var rexBuffer = pool.Rent(Math.Max(256, 8 * glob.Length));
        var patBuffer = pool.Rent(Math.Max(256, 8 * glob.Length));
        // escape the non-matches and translate the matches to regex equivalents
        var rexWriter = new SpanWriter(rexBuffer.AsSpan());
        // copy the non-matches and translate the matches to file system glob equivalents - * and ?
        var patWriter = new SpanWriter(patBuffer.AsSpan());

        try
        {
            // replace all wildcards with '*'
            foreach (Match match in matches)
            {
                // the non-match is from the current position to the start of the match - escape and copy the next non-match
                if (match.Index > globReader.Position)
                {
                    var nonMatch = globReader.Read(match.Index - globReader.Position);

                    RegexEscape(nonMatch, ref rexWriter);
                    patWriter.Write(nonMatch);
                }

                _ = globReader.Read(match.Length);  // consume the match

                // translate the match
                var (pat, rex) = TranslateGlobExpression(match);

                // TranslateGlobExpression(match); already processed this part, so just advance the reader
                rexWriter.Write(rex.AsSpan());
                patWriter.Write(pat.AsSpan());
            }

            // escape and copy the final non-match
            if (!globReader.IsEmpty)
            {
                var finalNonMatch = globReader.ReadAll();

                RegexEscape(finalNonMatch, ref rexWriter);
                patWriter.Write(finalNonMatch);
            }

            return (patWriter.Chars.ToString(),
                    rexWriter.Chars.ToString());
        }
        finally
        {
            pool.Return(rexBuffer);
            pool.Return(patBuffer);
        }
    }

    (string pattern, string regex) TranslateGlobExpression(Match match)
        // Match is one of the glob expression terms: *, ?, [class], [!class], where a <class> is a
        // sequence of letters (e.g. 'abc..'), letter ranges (e.g. '0-9'), and named classes (e.g. [:digit:]. We need to
        // identify and replace the glob expression in match with its respective regex equivalents:
        // * => .*
        // * => .
        // [class] => [class] (in each class we have to find and replace the named classes with their regex equivalent)
        // [!class] => [^class]
        // Therefore, we need a function that transforms each class into a .net regex - TransformClass
        => match
            .Groups
            .Values
            .FirstOrDefault(
                g => !string.IsNullOrWhiteSpace(g.Name) && !char.IsDigit(g.Name[0]) && !string.IsNullOrWhiteSpace(g.Value)) switch {
                    { Name: CharWildcardGr } question => (CharacterWildcard, _fileSystem.NameCharacter),
                    { Name: SeqWildcardGr } asterisk => (SequenceWildcard, _fileSystem.NameSequence),     // no need to filter the results
                    { Name: ClassGr } chrClass => (CharacterWildcard, $"[{TransformClass(chrClass.Value)}]"),
                    _ => throw new ArgumentException("Invalid glob match.", nameof(match)),
                };

    static ReadOnlySpan<char> TransformClass(string globClass)
    {
        var globReader = new SpanReader(globClass);

        var pool = ArrayPool<char>.Shared;
        var globBuffer = pool.Rent(Math.Max(256, 8 * globClass.Length));

        try
        {
            var globWriter = new SpanWriter(globBuffer.AsSpan());

            // the first char(s) can be '!' or ']' or '!]' that need special handling
            if (globReader.Peek() is '!')
            {
                _ = globReader.Read();  // consume it
                globWriter.Write(globReader.Remaining is > 1 ? '^' : '!'); // deal with the case of [!] vs [!class]
            }

            if (globReader.IsEmpty)
                return globWriter.Chars;

            if (globReader.Peek() is ']')
            {
                _ = globReader.Read();  // consume it
                globWriter.Write(']');
            }

            if (globReader.IsEmpty)
                return globWriter.Chars;

            var matches = NamedClassRegex().Matches(globClass);

            // replace all wildcards with '*'
            foreach (Match match in matches)
            {
                // the non-match is from the current position to the start of the match - escape and copy the next non-match
                if (match.Index > globReader.Position)
                    RegexEscape(globReader.Read(match.Index - globReader.Position), ref globWriter);

                _ = globReader.Read(match.Length);  // consume the match

                Debug.Assert(_namedClassTranslations.ContainsKey(match.Groups[ClassNameGr].Value), "We know this class name can be translated.");

                // get the class name, translate it and write it to the writer
                globWriter.Write(_namedClassTranslations[match.Groups[ClassNameGr].Value]);
            }

            // escape and copy the final non-match
            if (!globReader.IsEmpty)
                RegexEscape(globReader.ReadAll(), ref globWriter);

            return globWriter.Chars;
        }
        finally
        {
            pool.Return(globBuffer);
        }
    }

    static readonly FrozenDictionary<string, string> _namedClassTranslations =
        FrozenDictionary.ToFrozenDictionary(
            new Dictionary<string, string>()
            {
                ["alnum"] = @"\p{L}\p{Nd}\p{Nl}",
                ["alpha"] = @"\p{L}\p{Nl}",
                ["blank"] = @"\p{Zs}\t",
                ["cntrl"] = @"\p{Cc}",
                ["digit"] = @"\d",
                ["graph"] = @"\p{L}\p{M}\p{N}\p{P}\p{S}",
                ["lower"] = @"\p{Ll}\p{Lt}\p{Nl}",
                ["print"] = @"\p{S}\p{N}\p{Zs}\p{M}\p{L}\p{P}",
                ["punct"] = @"\p{P}$+<=>^`|~",
                ["space"] = @"\s",
                ["upper"] = @"\p{Lu}\p{Lt}\p{Nl}",
                ["xdigit"] = @"0-9A-Fa-f",
            });
}
