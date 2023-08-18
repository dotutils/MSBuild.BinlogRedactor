// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Microsoft.Build.BinlogRedactor
{
    public sealed class BinlogRedactorOptions: IOptions<BinlogRedactorOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinlogRedactorOptions"/> class.
        /// </summary>
        /// <param name="tokensToRedact">Tokens to be redacted from the binlog.</param>
        public BinlogRedactorOptions(string[] tokensToRedact) => TokensToRedact = tokensToRedact;

        public string[] TokensToRedact { get; private set; }
        public string? InputPath { get; init; }
        public string? OutputFileName { get; init; }
        public bool? DryRun { get; init; }
        public bool? OverWrite { get; init; }
        public bool? Recurse { get; init; }
        public bool? LogDetectedSecrets { get; init; }

        BinlogRedactorOptions IOptions<BinlogRedactorOptions>.Value => this;


        public BinlogRedactorOptions WithTokensToRedact(IEnumerable<string> tokensToRedact)
        {
            TokensToRedact = TokensToRedact.Concat(tokensToRedact).ToArray();
            return this;
        }

        public BinlogRedactorOptions WithTokenToRedact(string tokenToRedact)
        {
            TokensToRedact = TokensToRedact.Append(tokenToRedact).ToArray();
            return this;
        }
    }
}
