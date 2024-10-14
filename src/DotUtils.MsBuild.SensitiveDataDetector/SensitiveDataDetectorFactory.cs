// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DotUtils.MsBuild.SensitiveDataDetector;

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

    public static ISensitiveDataRedactor GetUserNameRedactor(bool identifyReplacements) => new UserNameDetector(identifyReplacements ? null : DefaultReplacementPattern);

    public static ISensitiveDataDetector GetUserNameDetector(bool identifyReplacements) => new UserNameDetector(identifyReplacements ? null : DefaultReplacementPattern);

    public static ISensitiveDataRedactor GetCommonSecretsRedactor(bool identifyReplacements) => new PatternsDetector(true, identifyReplacements ? null : DefaultReplacementPattern);

    public static ISensitiveDataDetector GetCommonSecretsDetector(bool identifyReplacements) => new PatternsDetector(true, identifyReplacements ? null : DefaultReplacementPattern);

    public static ISensitiveDataRedactor GetExplicitSecretsRedactor(string[] secretsToRedact, bool identifyReplacements) =>
        new ExplicitSecretsDetector(secretsToRedact, identifyReplacements ? null : DefaultReplacementPattern);

    public static ISensitiveDataDetector GetExplicitSecretsDetector(string[] secretsToRedact, bool identifyReplacements) =>
        new ExplicitSecretsDetector(secretsToRedact, identifyReplacements ? null : DefaultReplacementPattern);

    public static ISensitiveDataRedactor GetSecretsRedactor(
        SensitiveDataKind sensitiveDataKind,
        bool identifyReplacements,
        string[]? secretsToRedact = null)
    {
        List<ISensitiveDataRedactor> redactors = new List<ISensitiveDataRedactor>();

        if (secretsToRedact != null && secretsToRedact.Any() &&
            sensitiveDataKind.HasFlag(SensitiveDataKind.ExplicitSecrets))
        {
            redactors.Add(GetExplicitSecretsRedactor(secretsToRedact, identifyReplacements));
        }

        if (sensitiveDataKind.HasFlag(SensitiveDataKind.Username))
        {
            redactors.Add(GetUserNameRedactor(identifyReplacements));
        }

        if (sensitiveDataKind.HasFlag(SensitiveDataKind.CommonSecrets))
        {
            redactors.Add(GetCommonSecretsRedactor(identifyReplacements));
        }

        return new CompositeSecretsDetector(redactors.ToArray());
    }

    public static ISensitiveDataDetector GetSecretsDetector(
       SensitiveDataKind sensitiveDataKind,
       bool identifyReplacements,
       string[]? secretsToRedact = null)
    {
        List<ISensitiveDataDetector> detectors = new List<ISensitiveDataDetector>();

        if (secretsToRedact != null && secretsToRedact.Any() &&
            sensitiveDataKind.HasFlag(SensitiveDataKind.ExplicitSecrets))
        {
            detectors.Add(GetExplicitSecretsDetector(secretsToRedact, identifyReplacements));
        }

        if (sensitiveDataKind.HasFlag(SensitiveDataKind.Username))
        {
            detectors.Add(GetUserNameDetector(identifyReplacements));
        }

        if (sensitiveDataKind.HasFlag(SensitiveDataKind.CommonSecrets))
        {
            detectors.Add(GetCommonSecretsDetector(identifyReplacements));
        }

        return new CompositeSecretsDetector(detectors.ToArray());
    }
}
