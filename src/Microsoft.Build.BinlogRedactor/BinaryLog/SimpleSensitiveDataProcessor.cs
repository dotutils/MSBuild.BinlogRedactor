// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Build.BinlogRedactor.BinaryLog;

internal sealed class SimpleSensitiveDataProcessor : ISensitiveDataProcessor
{
    private readonly string[] _passwordsToRedact;

    public SimpleSensitiveDataProcessor(string[] passwordsToRedact)
    {
        _passwordsToRedact = passwordsToRedact;
    }

    public string ReplaceSensitiveData(string text)
    {
        foreach (string pwd in _passwordsToRedact)
        {
            text = text.Replace(pwd, "*******", StringComparison.CurrentCulture);
        }

        return text;
    }

    public bool IsSensitiveData(string text)
    {
        return _passwordsToRedact.Any(pwd => text.Contains(pwd, StringComparison.CurrentCulture));
    }
}
