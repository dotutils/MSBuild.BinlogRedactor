// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.Build.BinlogRedactor.BinaryLog;
using Microsoft.Build.BinlogRedactor.IO;
using Microsoft.Build.BinlogRedactor.Reporting;
using Microsoft.Build.BinlogRedactor.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Build.BinlogRedactor
{
    public sealed class BinlogRedactor
    {
        private readonly ILogger<BinlogRedactor> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IBinlogProcessor _binlogProcessor;

        public static void RegisterDefaultServices(IServiceCollection services)
        {
            services.AddSingleton<IFileSystem, PhysicalFileSystem>();
            services.AddSingleton<IBinlogProcessor, SimpleBinlogProcessor>();
            services.AddSingleton<ISensitiveDataProcessor, SimpleSensitiveDataProcessor>();
        }

        public BinlogRedactor(
            ILogger<BinlogRedactor> logger,
            IFileSystem fileSystem,
            IBinlogProcessor binlogProcessor)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _binlogProcessor = binlogProcessor;
        }

        /// <summary>
        /// Asynchronously performs the binlog redacting based on given configuration options.
        /// </summary>
        /// <param name="optionsAccessor"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<BinlogRedactorErrorCode> Execute(
            IOptions<BinlogRedactorOptions> optionsAccessor,
            CancellationToken cancellationToken = default)
        {
            BinlogRedactorOptions args = optionsAccessor.Value;

            if (args.DryRun ?? false)
            {
                throw new BinlogRedactorException("Dry run is not supported yet.",
                    BinlogRedactorErrorCode.NotYetImplementedScenario);
            }

            if (args.LogDetectedSecrets ?? false)
            {
                throw new BinlogRedactorException("Secrets logging is not supported yet.",
                    BinlogRedactorErrorCode.NotYetImplementedScenario);
            }

            if (args.TokensToRedact == null || args.TokensToRedact.Length == 0)
            {
                throw new BinlogRedactorException(
                    "At least one token to redact must be specified.",
                    BinlogRedactorErrorCode.NotEnoughInformationToProceed);
            }

            if (args.TokensToRedact.Any(s => s.Length <= 3))
            {
                throw new BinlogRedactorException(
                    "Passwords to redact must be nonempty and at least 4 characters long.",
                    BinlogRedactorErrorCode.InvalidOption);
            }

            if (string.IsNullOrEmpty(args.OutputFileName) && !(args.OverWrite ?? false))
            {
                throw new BinlogRedactorException(
                    "Output file must be specified if overwrite in place is not requested.",
                    BinlogRedactorErrorCode.NotEnoughInformationToProceed);
            }

            string[] inputFiles = GetInputFiles(args.InputPath, args.Recurse ?? false);
            bool hasMultipleFiles = inputFiles.Length > 1;

            if (hasMultipleFiles)
            {
                _logger.LogInformation("Found {count} binlog files. Will redact secrets in all. (found files: {files})",
                    inputFiles.Length, inputFiles.ToCsvString());
            }

            int fileOrderCount = 0;
            foreach (string inputFile in inputFiles)
            {
                string outputFile;
                if (string.IsNullOrEmpty(args.OutputFileName))
                {
                    outputFile = inputFile;
                }
                else
                {
                    outputFile = args.OutputFileName + (hasMultipleFiles ? (fileOrderCount++).ToString("D2") : null);
                }

                var result = await RedactWorker(inputFile, outputFile, args, cancellationToken).ConfigureAwait(false);

                // TODO: should we continue if there was an error?
                if (result != BinlogRedactorErrorCode.Success)
                {
                    if (fileOrderCount != inputFiles.Length)
                    {
                        _logger.LogInformation("Skipping redacting of remaining logs due to encountered error.");
                    }

                    return result;
                }
            }

            return BinlogRedactorErrorCode.Success;
        }

        private async Task<BinlogRedactorErrorCode> RedactWorker(
        string inputFile,
        string outputFile,
        BinlogRedactorOptions args,
        CancellationToken cancellationToken)
        {
            _logger.LogInformation("Redacting binlog {inputFile} to {outputFile} ({size} KB)", inputFile, outputFile, _fileSystem.GetFileSizeInBytes(inputFile) / 1024);

            bool replaceInPlace = inputFile.Equals(outputFile, StringComparison.CurrentCulture);
            if (replaceInPlace)
            {
                outputFile = Path.GetFileName(Path.GetTempFileName()) + ".binlog";
            }

            if ((args.OverWrite ?? false) && _fileSystem.FileExists(outputFile))
            {
                throw new BinlogRedactorException(
                    $"Requested output file [{outputFile}] exists, while overwrite option was not specified.",
                    BinlogRedactorErrorCode.FileSystemWriteFailed);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            var result = await _binlogProcessor.ProcessBinlog(inputFile, outputFile,
                new SimpleSensitiveDataProcessor(args.TokensToRedact!), cancellationToken).ConfigureAwait(false);

            stopwatch.Stop();
            _logger.LogInformation("Redacting done. Duration: {duration}", stopwatch.Elapsed);

            if (replaceInPlace)
            {
                _fileSystem.ReplaceFile(outputFile, inputFile);
            }

            return result;
        }

        private string[] GetInputFiles(string? inputPath, bool recurse)
        {
            string dirToSearch = inputPath ?? ".";

            if (!string.IsNullOrEmpty(inputPath) && !_fileSystem.DirectoryExists(inputPath))
            {
                if (!_fileSystem.FileExists(inputPath))
                {
                    throw new BinlogRedactorException($"Input path [{inputPath}] does not exist.",
                        BinlogRedactorErrorCode.InvalidData);
                }

                return new[] { inputPath };
            }

            string[] binlogs = _fileSystem.EnumerateFiles(dirToSearch, "*.binlog",
                new EnumerationOptions() { IgnoreInaccessible = true, RecurseSubdirectories = recurse, }).ToArray();

            if (binlogs.Length == 0)
            {
                throw new BinlogRedactorException(
                    $"No binlog file found in the current directory. Please specify the input file explicitly.",
                    BinlogRedactorErrorCode.NotEnoughInformationToProceed);
            }

            return binlogs;
        }

    }
}
