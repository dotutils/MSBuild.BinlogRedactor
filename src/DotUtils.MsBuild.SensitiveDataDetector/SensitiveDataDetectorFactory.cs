// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DotUtils.MsBuild.SensitiveDataDetector;

namespace Microsoft.Build.SensitiveDataDetector;

[Flags]
public enum SensitiveDataKind
{
    None = 0,
    Username = 1 << 0,
    CommonSecrets = 1 << 1,
    ExplicitSecrets = 1 << 2,

    // Specific types of sensitive data
    AzureClientId = 1 << 3,
    AzureClientSecret = 1 << 4,
    AzureContainerRegistryUsername = 1 << 5,
    AzureContainerRegistryPassword = 1 << 6,
    AzureServiceBusConnectionString = 1 << 7,
    AzureStorageAccountKey = 1 << 8,
    AzureStorageAccountNameAndKey = 1 << 9,
    AzureStorageConnectionString = 1 << 10,
    CosmosDbConnectionString = 1 << 11,
    GoogleApiKey = 1 << 12,
    GoogleOAuth = 1 << 13,
    JwtToken = 1 << 14,
    PasswordInUrl = 1 << 15,
    SlackToken = 1 << 16,
    SqlConnectionString = 1 << 17,

    // Groupings
    AllAzureSecrets = AzureClientId | AzureClientSecret | AzureContainerRegistryUsername | AzureContainerRegistryPassword |
                      AzureServiceBusConnectionString | AzureStorageAccountKey | AzureStorageAccountNameAndKey | AzureStorageConnectionString,
    AllGoogleSecrets = GoogleApiKey | GoogleOAuth,
    AllConnectionStrings = AzureServiceBusConnectionString | AzureStorageConnectionString | CosmosDbConnectionString | SqlConnectionString,

    All = ~None
}

public static class SensitiveDataDetectorFactory
{
    public static string DefaultReplacementPattern { get; set; } = "*******";

    public static ISensitiveDataRedactor GetUsernameRedactor(bool identifyReplacements) => new UsernameDetector(identifyReplacements ? null : DefaultReplacementPattern);

    public static ISensitiveDataDetector GetUsernameDetector(bool identifyReplacements) => new UsernameDetector(identifyReplacements ? null : DefaultReplacementPattern);

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
            redactors.Add(GetUsernameRedactor(identifyReplacements));
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
            detectors.Add(GetUsernameDetector(identifyReplacements));
        }

        if (sensitiveDataKind.HasFlag(SensitiveDataKind.CommonSecrets))
        {
            detectors.Add(GetCommonSecretsDetector(identifyReplacements));
        }

        return new CompositeSecretsDetector(detectors.ToArray());
    }
}
