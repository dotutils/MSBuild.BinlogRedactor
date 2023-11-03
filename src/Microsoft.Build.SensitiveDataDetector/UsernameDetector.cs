// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;

namespace Microsoft.Build.SensitiveDataDetector;

internal class UsernameDetector : ISensitiveDataRedactor
{
    public UsernameDetector() : this(defaultReplacementText) { }

    public UsernameDetector(string? replacementText) => this.replacementText =
        string.IsNullOrEmpty(replacementText) ? defaultReplacementText : replacementText!;

    private const RegexOptions regexOptions =
                    RegexOptions.Compiled |
                    RegexOptions.CultureInvariant;

    private const string winUsernamePathPattern = @".:\\[U|u]sers\\(.+?)(\\|\z)";
    private const string nixUsernamePathPattern = @"home\/(.+?)\/";
    private const string defaultReplacementText = @"REDACTED__Username";
    private readonly string replacementText;
    private string username = Environment.UserName;
    private bool usernameFound = false;

    private readonly Regex winUsernameRegex = new Regex(winUsernamePathPattern, regexOptions);
    private readonly Regex nixUsernameRegex = new Regex(nixUsernamePathPattern, regexOptions);


    public string Redact(string input)
    {
        if(!usernameFound)
        {
            DetectUsername(input, winUsernameRegex);
            DetectUsername(input, nixUsernameRegex);
        }

        return input.Replace(username, replacementText, StringComparison.InvariantCultureIgnoreCase);
    }

    private void DetectUsername(string input, Regex usernameRegex)
    {
        if (!usernameFound)
        {
            var match = usernameRegex.Match(input);
            if (match.Success)
            {
                username = match.Groups[1].Value;
                usernameFound = true;
            }
        }
    }
}
