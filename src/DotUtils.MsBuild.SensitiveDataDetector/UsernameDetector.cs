// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;
using DotUtils.MsBuild.SensitiveDataDetector;

namespace Microsoft.Build.SensitiveDataDetector;

internal class UserNameDetector(string? replacementText) : ISensitiveDataRedactor, ISensitiveDataDetector
{
    public UserNameDetector() : this(defaultReplacementText) { }

    private const RegexOptions regexOptions =
                    RegexOptions.Compiled |
                    RegexOptions.CultureInvariant;

    private const string winUsernamePathPattern = @".:\\[U|u]sers\\(.+?)(\\|\z)";
    private const string nixUsernamePathPattern = @"home\/(.+?)\/";
    private const string defaultReplacementText = @"REDACTED__Username";
    private readonly string replacementText =
        string.IsNullOrEmpty(replacementText) ? defaultReplacementText : replacementText!;
    private string username = Environment.UserName;
    private bool usernameFound = false;

    private readonly Regex winUsernameRegex = new Regex(winUsernamePathPattern, regexOptions);
    private readonly Regex nixUsernameRegex = new Regex(nixUsernamePathPattern, regexOptions);


    public string Redact(string input)
    {
        if (!usernameFound)
        {
            DetectUsername(input, winUsernameRegex);
            DetectUsername(input, nixUsernameRegex);
        }

        return input.Replace(username, replacementText, StringComparison.InvariantCultureIgnoreCase);
    }

    public Dictionary<SensitiveDataKind, List<SecretDescriptor>> Detect(string input)
    {
        var result = new Dictionary<SensitiveDataKind, HashSet<SecretDescriptor>>();
        var detectedUsernames = new HashSet<SecretDescriptor>();

        DetectUsernamesWithRegex(input, winUsernameRegex, detectedUsernames);
        DetectUsernamesWithRegex(input, nixUsernameRegex, detectedUsernames);

        DetectEnvironmentUsername(input, detectedUsernames);

        if (detectedUsernames.Count > 0)
        {
            result[SensitiveDataKind.Username] = detectedUsernames;
        }

        return result.ToDictionary(r => r.Key, r => r.Value.Select(hs => hs).ToList());
    }

    private void DetectUsernamesWithRegex(string input, Regex regex, HashSet<SecretDescriptor> detectedUsernames)
    {
        foreach (Match match in regex.Matches(input))
        {
            var username = match.Groups[1].Value;
            var lineInfo = StringUtils.GetLineAndColumn(input, match.Groups[1].Index);
            var secretDescriptor = new SecretDescriptor(username, lineInfo.lineNumber, lineInfo.columnNumber, match.Groups[1].Index);
            detectedUsernames.Add(secretDescriptor);

            if (!usernameFound)
            {
                this.username = username;
                usernameFound = true;
            }
        }
    }

    private void DetectEnvironmentUsername(string input, HashSet<SecretDescriptor> detectedUsernames)
    {
        int index = 0;
        while ((index = input.IndexOf(Environment.UserName, index, StringComparison.InvariantCultureIgnoreCase)) != -1)
        {
            var lineInfo = StringUtils.GetLineAndColumn(input, index);
            var secretDescriptor = new SecretDescriptor(Environment.UserName, lineInfo.lineNumber, lineInfo.columnNumber, index);
            detectedUsernames.Add(secretDescriptor);
            index += Environment.UserName.Length;

            if (!usernameFound)
            {
                username = Environment.UserName;
                usernameFound = true;
            }
        }
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

    private class SecretDescriptorComparer : IEqualityComparer<SecretDescriptor>
    {
        public bool Equals(SecretDescriptor x, SecretDescriptor y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            return x.GetType() != y.GetType() ? false : x.Secret == y.Secret && x.Index == y.Index;
        }

        public int GetHashCode(SecretDescriptor obj)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (obj.Secret?.GetHashCode() ?? 0);
                hash = hash * 23 + obj.Index.GetHashCode();
                return hash;
            }
        }
    }
}
