// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Build.SensitiveDataDetector;

namespace DotUtils.MsBuild.SensitiveDataDetector
{
    public interface ISensitiveDataDetector
    {
        Dictionary<SensitiveDataKind, List<SecretDescriptor>> Detect(string input);
    }

    public readonly record struct SecretDescriptor
    {
        public SecretDescriptor(string secret, int line, int column, int index, string? subKind = null)
        {
            Secret = secret;
            Line = line;
            Column = column;
            Index = index;
            SubKind = subKind ?? string.Empty;
        }

        public string? Secret { get; }

        public int Line { get; }

        public int Column { get; }

        public int Index { get; }

        public string SubKind { get; }
    }
}
