// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Build.SensitiveDataDetector;

internal static class CredentialsPatterns
{
    internal static readonly (string patternName, string regex)[] Patterns = new[]
    {
        ("Google-API-Key", @"AIza[A-Za-z0-9_\\\-]{35}"),
        ("Slack-Token", @"xox[pbar]\-[A-Za-z0-9]"),
        ("Azure-AD-Identity-Password", @"[0-9A-Za-z-_~.]{3}7Q~[0-9A-Za-z-_~.]{31}\b|\b[0-9A-Za-z-_~.]{3}8Q~[0-9A-Za-z-_~.]{34}"),
        //("Generic-Secret", @"(key|token|sig|secret|signature|password|passwd|pwd|android:value)[^a-zA-Z0-9]"),
        ("Email", @"[a-zA-Z0-9-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-]+"),
    };
}

internal class PatternsDetector : ISensitiveDataRedactor
{
    private const RegexOptions regexOptions =
        RegexOptions.Compiled |
        RegexOptions.Singleline |
        RegexOptions.CultureInvariant |
        // allow brackets within the pattern to be treated as literal characters
        RegexOptions.ExplicitCapture;

    private readonly List<(Regex Regex, string Replacement)> _patterns;

    public PatternsDetector(bool usePredefined = true, string? replacement = null)
    {
        if (usePredefined)
        {
            _patterns = CredentialsPatterns.Patterns
                .Select(p => (new Regex(p.regex, regexOptions), string.IsNullOrEmpty(replacement) ? $"REDACTED__{p.patternName}" : replacement!))
                .ToList();
        }
        else
        {
            _patterns = new List<(Regex Regex, string Replacement)>();
        }
    }

    public void AddPattern(string regex, string replacement)
    {
        _patterns.Add((new Regex(regex, regexOptions), replacement));
    }

    public string Redact(string input)
    {
        foreach ((Regex Regex, string Replacement) pattern in _patterns)
        {
            foreach (Match match in pattern.Regex.Matches(input))
            {
                string tokenToRedact = match.Groups[0].Value;

                input = input.Replace(tokenToRedact, pattern.Replacement);
            }
        }

        return input;
    }
}
