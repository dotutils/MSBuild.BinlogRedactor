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

        [Fact]
        public void PatternsDetector_RedactsAdditionalSecretTypes()
        {
            string replacement = "XXXXX";
            PatternsDetector detector = new PatternsDetector(true, replacement);

            detector.Redact("aws AKIAIOSFODNN7EXAMPLE here").Should().Be($"aws {replacement} here");
            detector.Redact("gitlab glpat-AAAAAAAAAAAAAAAAAAAA here").Should().Be($"gitlab {replacement} here");
            detector.Redact("stripe sk_live_" + "AAAAAAAAAAAAAAAAAAAAAAAA here").Should().Be($"stripe {replacement} here");
            detector.Redact("openai sk-AAAAAAAAAAAAAAAAAAAAAAAA here").Should().Be($"openai {replacement} here");
            detector.Redact("mailgun key-" + "00000000000000000000000000000000 here").Should().Be($"mailgun {replacement} here");
            detector.Redact("sendgrid SG.AAAAAAAAAAAAAAAAAAAAAA.AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA here").Should().Be($"sendgrid {replacement} here");
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
