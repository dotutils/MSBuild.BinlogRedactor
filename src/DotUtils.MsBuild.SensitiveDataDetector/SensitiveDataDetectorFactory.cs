// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DotUtils.MsBuild.SensitiveDataDetector;

namespace Microsoft.Build.SensitiveDataDetector;

[Flags]
public enum SensitiveDataKind : long
{
    None = 0L,
    Username = 1L << 0,
    CommonSecrets = 1L << 1,
    ExplicitSecrets = 1L << 2,
    CommonAnnotatedSecurityKey = 1L << 3,
    AadClientAppIdentifiableCredentials = 1L << 4,
    AzureFunctionIdentifiableKey = 1L << 5,
    AzureSearchIdentifiableQueryKey = 1L << 6,
    AzureSearchIdentifiableAdminKey = 1L << 7,
    AzureRelayIdentifiableKey = 1L << 8,
    AzureEventHubIdentifiableKey = 1L << 9,
    AzureServiceBusIdentifiableKey = 1L << 10,
    AzureIotHubIdentifiableKey = 1L << 11,
    AzureIotDeviceIdentifiableKey = 1L << 12,
    AzureIotDeviceProvisioningIdentifiableKey = 1L << 13,
    AzureStorageAccountIdentifiableKey = 1L << 14,
    AzureCosmosDBIdentifiableKey = 1L << 15,
    AzureBatchIdentifiableKey = 1L << 16,
    AzureMLWebServiceClassicIdentifiableKey = 1L << 17,
    AzureApimIdentifiableDirectManagementKey = 1L << 18,
    AzureApimIdentifiableSubscriptionKey = 1L << 19,
    AzureApimIdentifiableGatewayKey = 1L << 20,
    AzureApimIdentifiableRepositoryKey = 1L << 21,
    AzureCacheForRedisIdentifiableKey = 1L << 22,
    AzureContainerRegistryIdentifiableKey = 1L << 23,
    NuGetApiKey = 1L << 24,
    AzureDatabricksPat = 1L << 25,
    AzureEventGridIdentifiableKey = 1L << 26,
    NpmAuthorKey = 1L << 27,
    SecretScanningSampleToken = 1L << 28,
    AzureClientId = 1L << 29,
    AzureClientSecret = 1L << 30,
    AzureContainerRegistryUsername = 1L << 31,
    AzureContainerRegistryPassword = 1L << 32,
    AzureServiceBusConnectionString = 1L << 33,
    AzureStorageAccountKey = 1L << 34,
    AzureStorageAccountNameAndKey = 1L << 35,
    AzureStorageConnectionString = 1L << 36,
    CosmosDbConnectionString = 1L << 37,
    GoogleApiKey = 1L << 38,
    GoogleOAuth = 1L << 39,
    JwtToken = 1L << 40,
    PasswordInUrl = 1L << 41,
    SlackToken = 1L << 42,
    SqlConnectionString = 1L << 43
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
