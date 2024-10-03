// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DotUtils.MsBuild.BinlogRedactor.BinaryLog;
using DotUtils.MsBuild.SensitiveDataDetector;
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

internal sealed class SensitiveDataProcessor : ISensitiveDataProcessor
{
    private readonly ISensitiveDataRedactor _redactor;

    private readonly ISensitiveDataDetector _detector;

    public SensitiveDataProcessor(
        SensitiveDataKind sensitiveDataKind,
        bool identifyReplacements,
        string[]? secretsToRedact = null)
    {
        _redactor = SensitiveDataDetectorFactory.GetSecretsRedactor(sensitiveDataKind, identifyReplacements, secretsToRedact);
        _detector = SensitiveDataDetectorFactory.GetSecretsDetector(sensitiveDataKind, identifyReplacements, secretsToRedact);
    }

    public string ReplaceSensitiveData(string text) => _redactor.Redact(text);

    public Dictionary<SensitiveDataKind, List<SecretDescriptor>> DetectSensitiveData(string text) => _detector.Detect(text);

    public bool IsSensitiveData(string text) => _redactor.Redact(text) != text;
}
