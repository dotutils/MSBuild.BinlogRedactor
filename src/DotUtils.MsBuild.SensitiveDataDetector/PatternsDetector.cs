// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Reflection;
using System.Text.RegularExpressions;
using DotUtils.MsBuild.SensitiveDataDetector;

namespace Microsoft.Build.SensitiveDataDetector;

internal static class CredentialsPatterns
{
    // taken from https://github.com/microsoft/security-utilities/blob/main/GeneratedRegexPatterns/HighConfidenceSecurityModels.json
    internal static readonly (SensitiveDataKind kind, string patternName, string regex)[] Patterns =
    [
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
        (SensitiveDataKind.SlackToken, "Slack Token", @"xox[baprs]-[0-9]{12}-[0-9]{12}-[0-9]{12}-[a-z0-9]{32}"),
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
        foreach (var (_, regex, replacement) in _patterns)
        {
            input = regex.Replace(input, replacement);
        }
        return input;
    }

    private (SensitiveDataKind Kind, SecretDescriptor Descriptor) ChooseBestMatch(List<(SensitiveDataKind Kind, SecretDescriptor Descriptor)> matches) =>
        matches.OrderByDescending(m => m.Descriptor.Secret.Length).First();

}
