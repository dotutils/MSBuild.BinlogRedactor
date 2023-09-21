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
            UsernameDetector detector = new UsernameDetector(replacement);

            detector.Redact("user1").Should().Be("user1");
            detector.Redact(@"D:\Users\user1\.nuget\").Should().Be(@$"D:\Users\{replacement}\.nuget\");
            detector.Redact("user1").Should().Be(replacement);
            detector.Redact(@"D:\Users\user2\.nuget\").Should().Be(@"D:\Users\user2\.nuget\");
            detector.Redact(@"D:\Users\user1\.nuget\").Should().Be(@$"D:\Users\{replacement}\.nuget\");
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
    }
}
