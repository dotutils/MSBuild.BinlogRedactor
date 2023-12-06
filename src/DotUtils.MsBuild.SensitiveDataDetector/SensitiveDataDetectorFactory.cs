// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Build.SensitiveDataDetector;

[Flags]
public enum SensitiveDataKind
{
    Username = 1,
    CommonSecrets = 2,
    ExplicitSecrets = 4,
}

public static class SensitiveDataDetectorFactory
{
    public static string DefaultReplacementPattern { get; set; } = "*******";

    public static ISensitiveDataRedactor GetUsernameDetector(bool identifyReplacements)
    {
        return new UsernameDetector(identifyReplacements ? null : DefaultReplacementPattern);
    }

    public static ISensitiveDataRedactor GetCommonSecretsDetector(bool identifyReplacements)
    {
        return new PatternsDetector(true, identifyReplacements ? null : DefaultReplacementPattern);
    }

    public static ISensitiveDataRedactor GetExplicitSecretsDetector(string[] secretsToRedact, bool identifyReplacements)
    {
        return new ExplicitSecretsDetector(secretsToRedact, identifyReplacements ? null : DefaultReplacementPattern);
    }

    public static ISensitiveDataRedactor GetSecretsDetector(
        SensitiveDataKind sensitiveDataKind,
        bool identifyReplacements,
        string[]? secretsToRedact = null)
    {
        List<ISensitiveDataRedactor> redactors = new List<ISensitiveDataRedactor>();

        if (secretsToRedact != null && secretsToRedact.Any() &&
            sensitiveDataKind.HasFlag(SensitiveDataKind.ExplicitSecrets))
        {
            redactors.Add(GetExplicitSecretsDetector(secretsToRedact, identifyReplacements));
        }

        if (sensitiveDataKind.HasFlag(SensitiveDataKind.Username))
        {
            redactors.Add(GetUsernameDetector(identifyReplacements));
        }

        if (sensitiveDataKind.HasFlag(SensitiveDataKind.CommonSecrets))
        {
            redactors.Add(GetCommonSecretsDetector(identifyReplacements));
        }

        return new CompositeSecretsDetector(redactors.ToArray());
    }
}
