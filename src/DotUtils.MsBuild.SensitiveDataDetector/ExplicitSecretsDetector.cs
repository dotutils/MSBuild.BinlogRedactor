// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DotUtils.MsBuild.SensitiveDataDetector;

namespace Microsoft.Build.SensitiveDataDetector;


internal sealed class ExplicitSecretsDetector : ISensitiveDataRedactor, ISensitiveDataDetector
{
    private readonly (string pwd, string replacement)[] _passwordsToRedact;

    public ExplicitSecretsDetector(string[] passwordsToRedact, string? replacement = null)
    {
        _passwordsToRedact = passwordsToRedact.Select((pwd, cnt) =>
                (pwd, string.IsNullOrEmpty(replacement) ? $"REDACTED__PWD{(cnt + 1):00}" : replacement!))
            .ToArray();
    }

    public Dictionary<SensitiveDataKind, List<SecretDescriptor>> Detect(string input)
    {
        var result = new Dictionary<SensitiveDataKind, List<SecretDescriptor>>();
        var detectedPasswords = new List<SecretDescriptor>();

        foreach ((string pwd, _) in _passwordsToRedact)
        {
            int index = 0;
            while ((index = input.IndexOf(pwd, index, StringComparison.CurrentCulture)) != -1)
            {
                var lineInfo = StringUtils.GetLineAndColumn(input, index);
                var secretDescriptor = new SecretDescriptor
                {
                    Secret = pwd,
                    Line = lineInfo.lineNumber,
                    Column = lineInfo.columnNumber,
                    Index = index
                };
                detectedPasswords.Add(secretDescriptor);
                index += pwd.Length;
            }
        }

        if (detectedPasswords.Count > 0)
        {
            result[SensitiveDataKind.ExplicitSecrets] = detectedPasswords;
        }

        return result;
    }

    public string Redact(string input)
    {
        foreach ((string pwd, string replacement) pwd in _passwordsToRedact)
        {
            input = input.Replace(pwd.pwd, pwd.replacement, StringComparison.CurrentCulture);
        }

        return input;
    }
}
