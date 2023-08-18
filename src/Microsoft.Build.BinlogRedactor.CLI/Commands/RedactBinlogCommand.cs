// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.BinlogRedactor.BinaryLog;
using Microsoft.Build.BinlogRedactor.IO;
using Microsoft.Build.BinlogRedactor.Reporting;
using Microsoft.Build.BinlogRedactor.Utils;
using Microsoft.Extensions.Logging;

namespace Microsoft.Build.BinlogRedactor.Commands;

internal sealed class RedactBinlogCommand : ExecutableCommand<RedactBinlogCommandArgs, RedactBinlogCommandHandler>
{
    private const string CommandName = "redact-binlog";

    private readonly Option<string[]> _passwordsToRedactOption = new(new [] {"--password", "-p"})
    {
        Description = "Password or other sensitive data to be redacted from binlog. Multiple options are supported.",
        Arity = new ArgumentArity(1, 1000),
        IsRequired = true,
    };

    private readonly Option<string> _inputOption = new(new[] { "--input", "-i" })
    {
        Description = "Input binary log file name. Or a directory to inspect for all existing binlogs. If not specified current directory is assumed.",
        IsRequired = false,
    };

    private readonly Option<string> _outputFileOption = new(new[] { "--output", "-o" })
    {
        Description = "Output binary log file name. If not specified, replaces the input file in place - overwrite option needs to be specified in such case.",
        IsRequired = false,
    };

    private readonly Option<bool> _overWriteOption = new(new[] { "--overwrite", "-f" })
    {
        Description = "Replace the output file if it already exists. Replace the input file if the output file is not specified.",
    };

    private readonly Option<bool> _recurseOption = new(new[] { "--recurse", "-r" })
    {
        Description = "Recurse given path (or current dir if none) for all binlogs. Applies only when single input file is not specified.",
    };

    private readonly Option<bool> _dryRunOption = new(new[] { "--dryrun" })
    {
        Description = "Performs the operation in-memory and outputs what would be performed.",
    };

    private readonly Option<bool> _logSecretsOption = new(new[] { "--logsecrets"})
    {
        Description = "Logs what secrets have been detected and replaced. This should be used only for test/troubleshooting purposes!",
    };

    public RedactBinlogCommand() :
        base(CommandName, "Provides ability to redact sensitive data from MSBuild binlogs (https://aka.ms/binlog-redactor).")
    {
        AddOption(_passwordsToRedactOption);
        AddOption(_inputOption);
        AddOption(_outputFileOption);
        AddOption(_overWriteOption);
        AddOption(_dryRunOption);
        AddOption(_recurseOption);
        AddOption(_logSecretsOption);
    }

    protected internal override RedactBinlogCommandArgs ParseContext(ParseResult parseResult)
    {
        return new RedactBinlogCommandArgs(
            parseResult.GetValueForOption(_passwordsToRedactOption),
            parseResult.GetValueForOption(_inputOption),
            parseResult.GetValueForOption(_outputFileOption),
            parseResult.GetValueForOption(_dryRunOption),
            parseResult.GetValueForOption(_overWriteOption),
            parseResult.GetValueForOption(_recurseOption),
            parseResult.GetValueForOption(_logSecretsOption));
    }
}


internal sealed class RedactBinlogCommandHandler : ICommandExecutor<RedactBinlogCommandArgs>
{
    private readonly ILogger<RedactBinlogCommandHandler> _logger;
    private readonly BinlogRedactor _binlogRedactor;

    public RedactBinlogCommandHandler(
        ILogger<RedactBinlogCommandHandler> logger,
        BinlogRedactor binlogRedactor)
    {
        _logger = logger;
        _binlogRedactor = binlogRedactor;
    }

    public async Task<BinlogRedactorErrorCode> ExecuteAsync(
        RedactBinlogCommandArgs args,
        CancellationToken cancellationToken)
    {
        if (args.TokensToRedact == null || args.TokensToRedact.Length == 0)
        {
            throw new BinlogRedactorException(
                "At least one password to redact must be specified.",
                BinlogRedactorErrorCode.NotEnoughInformationToProceed);
        }

        BinlogRedactorOptions options = new(args.TokensToRedact)
        {
            InputPath = args.InputPath,
            OutputFileName = args.OutputFileName,
            DryRun = args.DryRun,
            OverWrite = args.OverWrite,
            Recurse = args.Recurse,
            LogDetectedSecrets = args.LogDetectedSecrets
        };

        return await _binlogRedactor.Execute(options, cancellationToken);
    }
}
