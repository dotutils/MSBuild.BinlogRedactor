// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Build.SensitiveDataDetector;

internal class CompositeSecretsDetector : ISensitiveDataRedactor
{
    private readonly ISensitiveDataRedactor[] _detectors;

    public CompositeSecretsDetector(params ISensitiveDataRedactor[] detectors)
    {
        _detectors = detectors;
    }

    public string Redact(string input)
    {
        foreach (ISensitiveDataRedactor detector in _detectors)
        {
            input = detector.Redact(input);
        }

        return input;
    }
}

