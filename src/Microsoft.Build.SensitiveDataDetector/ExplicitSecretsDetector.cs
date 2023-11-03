// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Build.SensitiveDataDetector;


internal sealed class ExplicitSecretsDetector : ISensitiveDataRedactor
{
    private readonly (string pwd, string replacement)[] _passwordsToRedact;

    public ExplicitSecretsDetector(string[] passwordsToRedact, string? replacement = null)
    {
        _passwordsToRedact = passwordsToRedact.Select((pwd, cnt) =>
                (pwd, string.IsNullOrEmpty(replacement) ? $"REDACTED__PWD{(cnt + 1):00}" : replacement!))
            .ToArray();
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
