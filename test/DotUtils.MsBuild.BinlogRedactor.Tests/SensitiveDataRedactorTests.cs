// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Build.SensitiveDataDetector;

namespace Microsoft.Build.BinlogRedactor.Tests
{
    public class SensitiveDataRedactorTests
    {
        [Fact]
        public void UsernameDetector_RedactsUsernames()
        {
            string replacement = "XXXXX";
            UserNameDetector detector = new UserNameDetector(replacement);

            detector.Redact("user1").Should().Be("user1");
            detector.Redact(@"D:\Users\user1\.nuget\").Should().Be(@$"D:\Users\{replacement}\.nuget\");
            detector.Redact("user1").Should().Be(replacement);
            detector.Redact(@"D:\Users\user2\.nuget\").Should().Be(@"D:\Users\user2\.nuget\");
            detector.Redact(@"D:\Users\user1\.nuget\").Should().Be(@$"D:\Users\{replacement}\.nuget\");
        }

        [Fact]
        public void UsernameDetector_RedactsUsernames_shortPath()
        {
            string replacement = "XXXXX";
            UserNameDetector detector = new UserNameDetector(replacement);

            detector.Redact("user1").Should().Be("user1");
            detector.Redact(@"D:\Users\user1").Should().Be(@$"D:\Users\{replacement}");
            detector.Redact("user1").Should().Be(replacement);
            detector.Redact(@"D:\Users\user2").Should().Be(@"D:\Users\user2");
            detector.Redact(@"D:\Users\user1").Should().Be(@$"D:\Users\{replacement}");
        }

        [Fact]
        public void PatternsDetector_RedactsDistinctSecretTypes()
        {
            string replacement = "XXXXX";
            PatternsDetector detector = new PatternsDetector(true, replacement);

            detector.Redact("blah blah blah").Should().Be("blah blah blah");
            detector.Redact("this is my key:AIzaSyBjekIVdzGQ1EZqnWJyxwNp5lkiQJWcppo blah").Should().Be($"this is my key:{replacement} blah");
            detector.Redact("""
this is my key:AIzaSyBjekIVdzGQ1EZqnWJyxwNp5lkiQJWcppo blah
and yet another one AIzaSyBoFYPiRofLUgoFbB_cKpyJkSbthwetvlc here
""").Should().Be($"""
this is my key:{replacement} blah
and yet another one {replacement} here
""");
        }

    [Theory]
    [InlineData("AWS Access Key ID", "AKIA", "IOSFODNN7EXAMPLE")]
    [InlineData("GitLab PAT", "glpat-", "abcdefghij0123456789")]
    [InlineData("Stripe key", "sk_live_", "4eC39HqLyjWDarjtT1zdp7dc")]
    [InlineData("OpenAI key", "sk-", "abcdefghijklmnopqrstuvwxyz0123456789ABCD")]
    [InlineData("Mailgun key", "key-", "0123456789abcdef0123456789abcdef")]
    public void PatternsDetector_RedactsAdditionalSecretTypes(string _, string prefix, string body)
    {
        string replacement = "XXXXX";
        PatternsDetector detector = new PatternsDetector(true, replacement);

        // Compose the sample secret at runtime to avoid embedding secret-like literals in source.
        string secret = prefix + body;

        detector.Redact($"value {secret} end").Should().Be($"value {replacement} end");
    }

    [Fact]
    public void PatternsDetector_RedactsSendGridApiKey()
    {
        string replacement = "XXXXX";
        PatternsDetector detector = new PatternsDetector(true, replacement);

        string secret = "SG." + new string('a', 22) + "." + new string('b', 43);

        detector.Redact($"value {secret} end").Should().Be($"value {replacement} end");
    }

    [Fact]
    public void PatternsDetector_RedactsPrivateKeyHeader()
    {
        string replacement = "XXXXX";
        PatternsDetector detector = new PatternsDetector(true, replacement);

        detector.Redact("-----BEGIN RSA PRIVATE KEY-----").Should().Be(replacement);
        detector.Redact("-----BEGIN PRIVATE KEY-----").Should().Be(replacement);
    }
    }
}
