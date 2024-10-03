// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Build.SensitiveDataDetector;

namespace DotUtils.MsBuild.SensitiveDataDetector
{
    public interface ISensitiveDataDetector
    {
        Dictionary<SensitiveDataKind, List<SecretDescriptor>> Detect(string input);
    }

    public record SecretDescriptor
    {
        public string? Secret { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public int Index { get; set; }
    }
}
