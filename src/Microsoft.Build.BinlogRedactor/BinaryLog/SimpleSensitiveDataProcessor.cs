// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Build.SensitiveDataDetector;

namespace Microsoft.Build.BinlogRedactor.BinaryLog;

internal sealed class SimpleSensitiveDataProcessor : ISensitiveDataProcessor
{
    public const string DefaultReplacementPattern = "*******";
    private readonly (string pwd, string replacement)[] _passwordsToRedact;

    public SimpleSensitiveDataProcessor(string[] passwordsToRedact, bool identifyReplacements)
    {
        _passwordsToRedact = passwordsToRedact.Select((pwd, cnt) =>
            (pwd, identifyReplacements ? $"REDACTED__PWD{(cnt + 1):00}" : DefaultReplacementPattern))
            .ToArray();
    }

    public string ReplaceSensitiveData(string text)
    {
        foreach ((string pwd, string replacement) pwd in _passwordsToRedact)
        {
            text = text.Replace(pwd.pwd, pwd.replacement, StringComparison.CurrentCulture);
        }

        return text;
    }

    public bool IsSensitiveData(string text)
    {
        return _passwordsToRedact.Any(pwd => text.Contains(pwd.pwd, StringComparison.CurrentCulture));
    }
}


internal sealed class AutoDetectedSensitiveDataProcessor : ISensitiveDataProcessor
{
    private readonly ISensitiveDataRedactor[] _redactors;

    public AutoDetectedSensitiveDataProcessor(bool identifyReplacements)
    {
        _redactors = new ISensitiveDataRedactor[]
        {
            new UsernameDetector(identifyReplacements ? null : SimpleSensitiveDataProcessor.DefaultReplacementPattern),
            new PatternsDetector(true, identifyReplacements ? null : SimpleSensitiveDataProcessor.DefaultReplacementPattern)
        };
    }

    public string ReplaceSensitiveData(string text)
    {
        foreach (ISensitiveDataRedactor sensitiveDataRedactor in _redactors)
        {
            text = sensitiveDataRedactor.Redact(text);
        }

        return text;
    }

    public bool IsSensitiveData(string text)
    {
        // TODO: Optimize or remove this method - it's not used anywhere
        return ReplaceSensitiveData(text) != text;
    }
}

internal sealed class CompositeSensitiveDataProcessor : ISensitiveDataProcessor
{
    private readonly ISensitiveDataProcessor[] _processors;

    public CompositeSensitiveDataProcessor(params ISensitiveDataProcessor[] processors)
    {
        _processors = processors;
    }

    public string ReplaceSensitiveData(string text)
    {
        foreach (ISensitiveDataProcessor processor in _processors)
        {
            text = processor.ReplaceSensitiveData(text);
        }

        return text;
    }

    public bool IsSensitiveData(string text)
    {
        return _processors.Any(processor => processor.IsSensitiveData(text));
    }
}

//TODO: pluggable sensitive data redactor - via specifying library and type name
