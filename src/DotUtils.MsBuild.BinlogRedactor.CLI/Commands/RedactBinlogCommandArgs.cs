﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Build.BinlogRedactor.Commands;

internal sealed class RedactBinlogCommandArgs
{
    public RedactBinlogCommandArgs(
        string[]? tokensToRedact,
        string? inputPath,
        string? outputFileName,
        bool? dryRun,
        bool? overWrite,
        bool? recurse,
        bool? logDetectedSecrets,
        bool? skipEmbeddedFiles,
        bool? identifyReplacemenets,
        bool? doNotAutodetectCommonPatterns)
    {
        TokensToRedact = tokensToRedact;
        InputPath = inputPath;
        OutputFileName = outputFileName;
        DryRun = dryRun;
        OverWrite = overWrite;
        Recurse = recurse;
        LogDetectedSecrets = logDetectedSecrets;
        SkipEmbeddedFiles = skipEmbeddedFiles;
        IdentifyReplacemenets = identifyReplacemenets;
        DoNotAutodetectCommonPatterns = doNotAutodetectCommonPatterns;
    }

    public string[]? TokensToRedact { get; init; }
    public string? InputPath { get; init; }
    public string? OutputFileName { get; init; }
    public bool? DryRun { get; init; }
    public bool? OverWrite { get; init; }
    public bool? Recurse { get; init; }
    public bool? LogDetectedSecrets { get; init; }
    public bool? SkipEmbeddedFiles { get; init; }
    public bool? IdentifyReplacemenets { get; init; }
    // TODO: this will need more detailed configurability. Plus we need pluggability.
    public bool? DoNotAutodetectCommonPatterns { get; init; }
}

