using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.Build.BinlogRedactor.BinaryLog;
using Microsoft.Build.BinlogRedactor.IO;
using Microsoft.Build.BinlogRedactor.Reporting;
using Microsoft.Build.Logging;
using Microsoft.Extensions.Logging;

namespace Microsoft.Build.BinlogRedactor.Tests
{
    [UsesVerify]
    public class BinlogRedactorTests : IClassFixture<TestLoggerFactory>
    {
        private readonly ILoggerFactory _loggerFactory;

        public BinlogRedactorTests(TestLoggerFactory testLoggerFactory)
        {
            _loggerFactory = testLoggerFactory;
        }

        static BinlogRedactorTests()
        {
            // TODO: this is a workaround until the MSBuild changes for reproducible binlogs
            //  are published and we can use them in the tests (via dotnet sdk image).
            // We have pre-created binlogs - but those are reproducible only for given OS/sdk
            //  (mainly due to zipping).
            foreach (string binlogFile in GetBinlogFiles())
            {
                ReplaceBinlogWithReplayed(binlogFile);
            }
            // Let's skip this heck for no-op redaction as we test deserialized equality.
            // ReplaceBinlogWithReplayed(Path.Combine("assets", "console.binlog"));
        }

        private static void ReplaceBinlogWithReplayed(string binlogPath)
        {
            string replayedFile = binlogPath + "temp.binlog";

            BinaryLogReplayEventSource replayEventSource = new BinaryLogReplayEventSource();
            BuildEventArgsReader buildEventsReader =
                BinaryLogReplayEventSource.OpenBuildEventsReader(binlogPath);
            BinaryLogger outputBinlog = new BinaryLogger()
            {
                Parameters = $"LogFile={replayedFile};ProjectImports=Replay;OmitInitialInfo",
            };
            // Subscribe empty action. But the mere subscribing forces unpacking and repacking of embedded files
            buildEventsReader.ArchiveFileEncountered += arg => {};
            outputBinlog.Initialize(replayEventSource);
            replayEventSource.Replay(buildEventsReader, CancellationToken.None);
            outputBinlog.Shutdown();
            buildEventsReader.Dispose();

            new PhysicalFileSystem().ReplaceFile(replayedFile, binlogPath);
        }

        private static IEnumerable<string> GetBinlogFiles([CallerFilePath] string sourceFile = "")
        {
            if (string.IsNullOrEmpty(sourceFile))
            {
                return Enumerable.Empty<string>();
            }

            string dir = Path.Combine(Path.GetDirectoryName(sourceFile)!, VerifyInitialization.SnapshotsDirectory);

            return Directory.EnumerateFiles(dir, "*.binlog",
                new EnumerationOptions() { IgnoreInaccessible = true, RecurseSubdirectories = true });
        }

        [Fact]
        public async Task ExecuteIntegrationTest_NoOpRedactionShouldNotChangeFile()
        {
            Environment.SetEnvironmentVariable("MSBUILDDETERMNISTICBINLOG", "1");

            string outputFile = "console-redacted-01.binlog";
            string inputFile = Path.Combine("assets", "console.binlog");

            File.Exists(inputFile).Should().BeTrue();
            File.Delete(outputFile);

            BinlogRedactorOptions options = new BinlogRedactorOptions(new string[] { Guid.NewGuid().ToString() })
            {
                InputPath = inputFile, OutputFileName = outputFile, OverWrite = false,
                DoNotAutodetectCommonPatterns = true,
            };
            // Will delete file at the end of function
            using FileDeletingScope fileDeletingScope = new FileDeletingScope(outputFile);
            BinlogRedactor binlogRedactor = new BinlogRedactor(_loggerFactory.CreateLogger<BinlogRedactor>(),
                new PhysicalFileSystem(), new SimpleBinlogProcessor());

            (await binlogRedactor.Execute(options).ConfigureAwait(false)).Should()
                .Be(BinlogRedactorErrorCode.Success);

            File.Exists(outputFile).Should().BeTrue();

            BinlogComparer.AssertBinlogsHaveEqualContent(inputFile, outputFile);

            // This check will fail on CI unless we create the binlog on same runtime.
            // BinlogComparer.AssertFilesAreBinaryEqual(inputFile, outputFile);
        }

        [Fact]
        public async Task ExecuteIntegrationTest_RedactionShouldNotChangeOtherPartsOfFile()
        {
            Environment.SetEnvironmentVariable("MSBUILDDETERMNISTICBINLOG", "1");

            string outputFile = "console-redacted-02.binlog";
            File.Delete(outputFile);
            string inputFile = Path.Combine("assets", "console.binlog");

            File.Exists(inputFile).Should().BeTrue();

            BinlogRedactorOptions options = new BinlogRedactorOptions(new string[] { "restore", "console" })
            {
                InputPath = inputFile, OutputFileName = outputFile, OverWrite = false,
                IdentifyReplacemenets = true,
                // DoNotAutodetectCommonPatterns = true,
            };
            //using FileDeletingScope fileDeletingScope = new FileDeletingScope(outputFile);
            BinlogRedactor binlogRedactor = new BinlogRedactor(_loggerFactory.CreateLogger<BinlogRedactor>(),
                new PhysicalFileSystem(), new SimpleBinlogProcessor());
            (await binlogRedactor.Execute(options).ConfigureAwait(false)).Should().Be(BinlogRedactorErrorCode.Success);

            File.Exists(outputFile).Should().BeTrue();

            await VerifyFile(outputFile).ConfigureAwait(false);
        }

        private sealed class FileDeletingScope : IDisposable
        {
            private readonly string _file;

            public FileDeletingScope(string file)
            {
                _file = file;
            }

            public void Dispose()
            {
                if (File.Exists(_file))
                {
                    File.Delete(_file);
                }
            }
        }
    }
}
