// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Text.RegularExpressions;
using DotUtils.MsBuild.SensitiveDataDetector;

namespace Microsoft.Build.SensitiveDataDetector;

internal static class CredentialsPatterns
{
    private const RegexOptions regexOptions =
        RegexOptions.Compiled |
        RegexOptions.Singleline |
        RegexOptions.CultureInvariant |
        RegexOptions.ExplicitCapture;

    // taken from https://github.com/microsoft/security-utilities/blob/main/GeneratedRegexPatterns/HighConfidenceSecurityModels.json
    internal static readonly List<(string patternName, Regex regex)> s_patterns = new List<(string patternName, Regex regex)>()
    {
         ("Common Annotated Security Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<secret>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{52}JQQJ9(?:9|D|H)[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890][A-L][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{16}[A-Za-z][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{7}(?:[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{2}==)?)", regexOptions)),
         ("AadClientApp Identifiable Credentials", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\\-_~.+/=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\\-_~.]{3}(7|8)Q~[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\\-_~.]{31,34})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\\-_~.+/=]|$)}", regexOptions)),
         ("Azure Function Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\\-_])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\\-_]{44}AzFu[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\\-_]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890\\-_]|$)", regexOptions)),
         ("Azure Search Identifiable Query Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{42}AzSe[A-D][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{5})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure Search Identifiable Admin Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{42}AzSe[A-D][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{5})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure Relay Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}\\+ARm[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure EventHub Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}\\+AEh[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure ServiceBus Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}\\+ASb[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure IotHub Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}AIoT[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure IotDevice Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}AIoT[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure IotDevice Provisioning Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}AIoT[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure Storage Account Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}\\+ASt[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure CosmosDB IdentifiableKey", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}ACDb[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure Batch Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}\\+ABa[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure ML Web Service Classic Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}\\+AMC[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure Apim Identifiable Direct Management Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}APIM[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure Apim Identifiable SubscriptionKey", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}APIM[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure Apim Identifiable Gateway Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}APIM[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure Apim Identifiable Repository Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{76}APIM[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}[AQgw]==)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure Cache For Redis Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}AzCa[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Azure Container Registry Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{42}\\+ACR[A-D][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("NuGet Api Key", new Regex(@"(^|[^0-9a-z])(?<refine>oy2[a-p][0-9a-z]{15}[aq][0-9a-z]{11}[eu][bdfhjlnprtvxz357][a-p][0-9a-z]{11}[aeimquy4])([^aeimquy4]|$)", regexOptions)),
         ("Azure Databricks Pat", new Regex(@"(?:^|[^0-9a-f\\-])(?<refine>dapi[0-9a-f\\-]{32,34})(?:[^0-9a-f\\-]|$)", regexOptions)),
         ("Azure Event Grid Identifiable Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=])(?<refine>[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{33}AZEG[A-P][abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/]{5}=)([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890+/-_=]|$)", regexOptions)),
         ("Npm Author Key", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890])(?<refine>npm_[abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]{36})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]|$)", regexOptions)),
         ("Secret Scanning Sample Token", new Regex(@"(^|[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890])(?<secret>secret_scanning_ab85fc6f8d7638cf1c11da812da308d43_[0-9A-Za-z]{5})([^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890]|$)", regexOptions)),
         ("Google Api Key", new Regex(@"AIza[A-Za-z0-9_\\\-]{35}", regexOptions)),
         ("Slack Token", new Regex(@"xox[pbar]\-[A-Za-z0-9]", regexOptions)),
         ("Azure Ad Identity Password", new Regex(@"[0-9A-Za-z-_~.]{3}7Q~[0-9A-Za-z-_~.]{31}\b|\b[0-9A-Za-z-_~.]{3}8Q~[0-9A-Za-z-_~.]{34}", regexOptions)),
         ("Email", new Regex(@"[a-zA-Z0-9-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-]+", regexOptions)),
         ("GitHub Token", new Regex(@"(?:^|(?<=\W))(gh[ps]_[a-zA-Z0-9]{36}|github_pat_[a-zA-Z0-9]{22}_[a-zA-Z0-9]{59})(?=$|\W)", regexOptions))
    };
}

internal class PatternsDetector(bool usePredefined = true, string? replacement = null) : ISensitiveDataRedactor, ISensitiveDataDetector
{
    private readonly List<(Regex Regex, string Replacement)> _patterns = usePredefined
            ? CredentialsPatterns.s_patterns
                .Select(p => (p.regex, string.IsNullOrEmpty(replacement) ? $"REDACTED__{p.patternName}" : replacement!))
                .ToList()
            : (List<(Regex Regex, string Replacement)>)([]);

    public void AddPattern(string patternName, Regex regex, string replacement) => _patterns.Add((regex, replacement));

    public Dictionary<SensitiveDataKind, List<SecretDescriptor>> Detect(string input)
    {
        var result = new Dictionary<SensitiveDataKind, List<SecretDescriptor>>();

        foreach (var (patternName, regex) in CredentialsPatterns.s_patterns)
        {
            MatchCollection matches = regex.Matches(input);
            foreach (Match match in matches)
            {
                var lineInfo = StringUtils.GetLineAndColumn(input, match.Index);
                var secretDescriptor = new SecretDescriptor(match.Value, lineInfo.lineNumber, lineInfo.columnNumber, match.Index, patternName);
                if (!result.ContainsKey(SensitiveDataKind.CommonSecrets))
                {
                    result[SensitiveDataKind.CommonSecrets] = [];
                }

                result[SensitiveDataKind.CommonSecrets].Add(secretDescriptor);
                break;
            }
        }

        return result;
    }

    public string Redact(string input)
    {
        foreach ((Regex regex, string replacement) in _patterns)
        {
            input = regex.Replace(input, replacement);
        }
        return input;
    }
}
