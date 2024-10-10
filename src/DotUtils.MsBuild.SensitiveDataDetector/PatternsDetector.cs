// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Text.RegularExpressions;
using DotUtils.MsBuild.SensitiveDataDetector;

namespace Microsoft.Build.SensitiveDataDetector;

internal static class CredentialsPatterns
{
    // taken from https://github.com/microsoft/security-utilities/blob/main/GeneratedRegexPatterns/HighConfidenceSecurityModels.json
    internal static readonly (SensitiveDataKind kind, string patternName, string regex)[] Patterns =
    [
        (SensitiveDataKind.SlackToken, "Slack-Token", @"xox[pbar]\-[A-Za-z0-9]"),
        (SensitiveDataKind.AzureAdIdentityPassword, "Azure-AD-Identity-Password", @"[0-9A-Za-z-_~.]{3}7Q~[0-9A-Za-z-_~.]{31}\b|\b[0-9A-Za-z-_~.]{3}8Q~[0-9A-Za-z-_~.]{34}"),
        (SensitiveDataKind.Email, "Email", @"[a-zA-Z0-9-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-]+"),
        (SensitiveDataKind.CommonAnnotatedSecurityKey, "CommonAnnotatedSecurityKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<secret>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{52}JQQJ9(?:9|D|H)[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890][A-L][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{16}[A-Za-z][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{7}(?:[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{2}==)?)"),
        (SensitiveDataKind.AadClientAppIdentifiableCredentials, "AadClientAppIdentifiableCredentials", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\-_~.+/=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\-_~.]{3}(7|8)Q~[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\-_~.]{31,34})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\-_~.+/=]|$)"),
        (SensitiveDataKind.AzureFunctionIdentifiableKey, "AzureFunctionIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\-_])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\-_]{44}AzFu[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\-_]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\-_]|$)"),
        (SensitiveDataKind.AzureSearchIdentifiableQueryKey, "AzureSearchIdentifiableQueryKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{42}AzSe[A-D][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{5})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureSearchIdentifiableAdminKey, "AzureSearchIdentifiableAdminKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{42}AzSe[A-D][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{5})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureRelayIdentifiableKey, "AzureRelayIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}\+ARm[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureEventHubIdentifiableKey, "AzureEventHubIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}\+AEh[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureServiceBusIdentifiableKey, "AzureServiceBusIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}\+ASb[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureIotHubIdentifiableKey, "AzureIotHubIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}AIoT[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureIotDeviceIdentifiableKey, "AzureIotDeviceIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}AIoT[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureIotDeviceProvisioningIdentifiableKey, "AzureIotDeviceProvisioningIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}AIoT[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureStorageAccountIdentifiableKey, "AzureStorageAccountIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}\+ASt[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureCosmosDBIdentifiableKey, "AzureCosmosDBIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}ACDb[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureBatchIdentifiableKey, "AzureBatchIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}\+ABa[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureMLWebServiceClassicIdentifiableKey, "AzureMLWebServiceClassicIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}\+AMC[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureMLWebServiceClassicIdentifiableKey, "AzureMLWebServiceClassicIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}\+AMC[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureApimIdentifiableDirectManagementKey, "AzureApimIdentifiableDirectManagementKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}APIM[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureApimIdentifiableSubscriptionKey, "AzureApimIdentifiableSubscriptionKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}APIM[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureApimIdentifiableGatewayKey, "AzureApimIdentifiableGatewayKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}APIM[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureApimIdentifiableRepositoryKey, "AzureApimIdentifiableRepositoryKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}APIM[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureCacheForRedisIdentifiableKey, "AzureCacheForRedisIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}AzCa[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.AzureContainerRegistryIdentifiableKey, "AzureContainerRegistryIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{42}\+ACR[A-D][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.NuGetApiKey, "NuGetApiKey", @"(^|[^0-9a-z])(?<refine>oy2[a-p][0-9a-z]{15}[aq][0-9a-z]{11}[eu][bdfhjlnprtvxz357][a-p][0-9a-z]{11}[aeimquy4])([^aeimquy4]|$)"),
        (SensitiveDataKind.AzureDatabricksPat, "AzureDatabricksPat", @"(?:^|[^0-9a-f\-])(?<refine>dapi[0-9a-f\-]{32,34})(?:[^0-9a-f\-]|$)"),
        (SensitiveDataKind.AzureEventGridIdentifiableKey, "AzureEventGridIdentifiableKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}AZEG[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)"),
        (SensitiveDataKind.NpmAuthorKey, "NpmAuthorKey", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890])(?<refine>npm_[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{36})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]|$)"),
        (SensitiveDataKind.SecretScanningSampleToken, "SecretScanningSampleToken", @"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890])(?<secret>secret_scanning_ab85fc6f8d7638cf1c11da812da308d43_[0-9A-Za-z]{5})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]|$)"),
        (SensitiveDataKind.AzureClientId, "Azure Client ID", @"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"),
        (SensitiveDataKind.AzureClientSecret, "Azure Client Secret", @"[0-9A-Za-z\-]{30}"),
        (SensitiveDataKind.AzureContainerRegistryUsername, "Azure Container Registry Username", @"[a-zA-Z0-9]+\.azurecr\.io"),
        (SensitiveDataKind.AzureContainerRegistryPassword, "Azure Container Registry Password", @"[0-9A-Za-z\-=]{30,100}"),
        (SensitiveDataKind.AzureServiceBusConnectionString, "Azure Service Bus Connection String", @"Endpoint=sb://[^/]+\.servicebus\.windows\.net/;SharedAccessKeyName=[^;]+;SharedAccessKey=[^\s]+"),
        (SensitiveDataKind.AzureStorageAccountKey, "Azure Storage Account Key", @"[a-zA-Z0-9+/=]{88}"),
        (SensitiveDataKind.AzureStorageAccountNameAndKey, "Azure Storage Account Name and Key", @"DefaultEndpointsProtocol=https?;AccountName=[^;]+;AccountKey=[a-zA-Z0-9+/=]{86}==;EndpointSuffix=core\.windows\.net"),
        (SensitiveDataKind.AzureStorageConnectionString, "Azure Storage Connection String", @"DefaultEndpointsProtocol=https?;AccountName=[^;]+;AccountKey=[^;]+;"),
        (SensitiveDataKind.CosmosDbConnectionString, "Cosmos DB Connection String", @"AccountEndpoint=https://[^:]+\.documents\.azure\.com:443/;AccountKey=[^;]+;"),
        (SensitiveDataKind.GoogleApiKey, "Google API Key", @"AIza[0-9A-Za-z\-_]{35}"),
        (SensitiveDataKind.GoogleOAuth, "Google OAuth", @"[0-9]+-[0-9A-Za-z_]{32}\.apps\.googleusercontent\.com"),
        (SensitiveDataKind.JwtToken, "JWT Token", @"eyJ[a-zA-Z0-9\-_]+\.[a-zA-Z0-9\-_]+\.[a-zA-Z0-9\-_]+"),
        (SensitiveDataKind.PasswordInUrl, "Password in URL", @"[a-zA-Z]{3,10}://[^/\s:@]*?:[^/\s:@]*?@[^/\s:@]*"),
        (SensitiveDataKind.SqlConnectionString, "SQL Connection String", @"Server=[^;]+;(Database=[^;]+;)?(User Id=[^;]+;)?(Password=[^;]+;)?")
        ];
    }

internal class PatternsDetector : ISensitiveDataRedactor, ISensitiveDataDetector
{
    private const RegexOptions regexOptions =
        RegexOptions.Compiled |
        RegexOptions.Singleline |
        RegexOptions.CultureInvariant |
        RegexOptions.ExplicitCapture;

    private readonly List<(SensitiveDataKind Kind, Regex Regex, string Replacement)> _patterns;

    public PatternsDetector(bool usePredefined = true, string? replacement = null)
    {
        if (usePredefined)
        {
            _patterns = CredentialsPatterns.Patterns
                .Select(p => (p.kind, new Regex(p.regex, regexOptions), string.IsNullOrEmpty(replacement) ? $"REDACTED__{p.patternName}" : replacement!))
                .ToList();
        }
        else
        {
            _patterns = new List<(SensitiveDataKind, Regex, string)>();
        }
    }

    public void AddPattern(SensitiveDataKind kind, string regex, string replacement) => _patterns.Add((kind, new Regex(regex, regexOptions), replacement));

    public Dictionary<SensitiveDataKind, List<SecretDescriptor>> Detect(string input)
    {
        var result = new Dictionary<SensitiveDataKind, List<SecretDescriptor>>();
        var allMatches = new List<(SensitiveDataKind Kind, SecretDescriptor Descriptor)>();

        foreach (var (kind, regex, _) in _patterns)
        {
            var matches = regex.Matches(input);
            foreach (Match match in matches)
            {
                var lineInfo = StringUtils.GetLineAndColumn(input, match.Index);
                var secretDescriptor = new SecretDescriptor(match.Value, lineInfo.lineNumber, lineInfo.columnNumber, match.Index);
                allMatches.Add((kind, secretDescriptor));
            }
        }

        // Group matches by their position in the file
        var groupedMatches = allMatches.GroupBy(m => (m.Descriptor.Line, m.Descriptor.Column));

        foreach (var group in groupedMatches)
        {
            var bestMatch = ChooseBestMatch(group.ToList());
            if (!result.ContainsKey(bestMatch.Kind))
            {
                result[bestMatch.Kind] = new List<SecretDescriptor>();
            }

            result[bestMatch.Kind].Add(bestMatch.Descriptor);
        }

        return result;
    }

    public string Redact(string input)
    {
        var t = Detect(input);
        foreach (var (_, regex, replacement) in _patterns)
        {
            input = regex.Replace(input, replacement);
        }
        return input;
    }

    private (SensitiveDataKind Kind, SecretDescriptor Descriptor) ChooseBestMatch(List<(SensitiveDataKind Kind, SecretDescriptor Descriptor)> matches) =>
        matches.OrderByDescending(static m => m.Descriptor.Secret.Length).First();

}
