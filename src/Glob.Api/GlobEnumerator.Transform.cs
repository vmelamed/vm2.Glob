// SPDX-License-Identifier: MIT
// Copyright (c) 2025-2026 Val Melamed

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

        // go the hard way
        var (pattern, regex) = pr;
        var matches = GlobExpressionRegex().Matches(glob);
        // now we can look at the glob expression as a sequence of terms: <non-match><match>*<non-match> where each non-match
        // element can be empty and each match element is a single glob character: *, ?, [class], [!class]

        if (matches.Count is 0)
        {
            // no pattern has no glob characters
            if (!glob.AsSpan().ContainsAny(RegexChars))
                return (glob, "");  // no need to regex-escape a pattern with no glob characters

            // we need to regex-escape the characters of the pattern
            var writer = new SpanWriter(stackalloc char[2 * glob.Length]);
            RegexEscape(glob.AsSpan(), ref writer);
            return (glob, writer.Chars.ToString());
        }

        // now the pattern can be thought of as a sequence of plain-character sub-sequences and a glob-character sub-sequences
        // ( '?' '*' '[:class:]' '[!class]'), where each plain-chars sub-sequence can be empty. In EBNF:
        // pattern ::= (<plain-chars><glob-chars>)*<plain-chars>
        // plain-chars ::= <any sequence of characters that are not glob characters>
        // glob-chars ::= '?' '*' '[:class:]' '[!class:]'

        // span to go through the glob
        var globReader = new SpanReader(glob.AsSpan());
        var pool = ArrayPool<char>.Shared;
        var patBuffer = pool.Rent(Math.Max(128, glob.Length * 2));
        var rexBuffer = pool.Rent(Math.Max(256, glob.Length * _fileSystem.MaxNameCharRegexLength));
        // copy the non-matches and translate the matches to file system glob equivalents - * and ?
        var patWriter = new SpanWriter(patBuffer.AsSpan());
        // escape the non-matches and translate the matches to regex equivalents
        var rexWriter = new SpanWriter(rexBuffer.AsSpan());

        try
        {
            foreach (Match match in matches)
            {
                // the non-match is from the current position to the start of the match
                if (match.Index > globReader.Position)
                {
                    var nonMatch = globReader.Read(match.Index - globReader.Position);

                    patWriter.Write(nonMatch);
                    // escape the non-match for the regex
                    RegexEscape(nonMatch, ref rexWriter);
                }

                _ = globReader.Read(match.Length);  // consume the match

                // translate the match
                var (pat, rex) = TranslateGlobExpression(match);

                patWriter.Write(pat.AsSpan());
                rexWriter.Write(rex.AsSpan());
            }

            // escape and copy the final non-match
            if (!globReader.IsEmpty)
            {
                var finalNonMatch = globReader.ReadAll();

                patWriter.Write(finalNonMatch);
                // escape the non-match for the regex
                RegexEscape(finalNonMatch, ref rexWriter);
            }

            return (patWriter.Chars.ToString(),
                    rexWriter.Chars.ToString());
        }
        finally
        {
            pool.Return(patBuffer);
            pool.Return(rexBuffer);
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
            .FirstOrDefault(g => !string.IsNullOrWhiteSpace(g.Name) && !char.IsDigit(g.Name[0]) && !string.IsNullOrWhiteSpace(g.Value))
                switch {
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
