using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Build.BinlogRedactor.BinaryLog;
using Microsoft.Build.BinlogRedactor.IO;
using Microsoft.Build.BinlogRedactor.Reporting;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Microsoft.Build.BinlogRedactor.Tests
{
    [UsesVerify]
    public class BinlogRedactorTests: IClassFixture<TestLoggerFactory>
    {
        private readonly ILoggerFactory _loggerFactory;

        public BinlogRedactorTests(TestLoggerFactory testLoggerFactory)
        {
            _loggerFactory = testLoggerFactory;
        }

        [Fact]
        public async Task ExecuteIntegrationTest_NoOpRedactionShouldNotChangeFile()
        {
            string outputFile = "output.binlog";
            File.Delete(outputFile);
            string inputFile = Path.Combine("assets", "console.binlog");

            File.Exists(inputFile).Should().BeTrue();

            BinlogRedactorOptions options = new BinlogRedactorOptions(new string[] { Guid.NewGuid().ToString() })
            {
                InputPath = inputFile, OutputFileName = outputFile, OverWrite = false,
            };
            // Will delete file at the end of function
            using FileDeletingScope fileDeletingScope = new FileDeletingScope(outputFile);
            BinlogRedactor binlogRedactor = new BinlogRedactor(_loggerFactory.CreateLogger<BinlogRedactor>(),
                new PhysicalFileSystem(), new SimpleBinlogProcessor());
            (await binlogRedactor.Execute(options).ConfigureAwait(false)).Should().Be(BinlogRedactorErrorCode.Success);

            File.Exists(outputFile).Should().BeTrue();

            // This is currently failing as the redaction does not preserve content of the original file
            FilesAreBinaryEqual(new FileInfo(inputFile), new FileInfo(outputFile)).Should().BeTrue();
        }

        private static bool FilesAreBinaryEqual(FileInfo first, FileInfo second)
        {
            if (first.Length != second.Length)
                return false;

            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            using FileStream fs1 = first.OpenRead();
            using FileStream fs2 = second.OpenRead();
            for (int i = 0; i < first.Length; i++)
            {
                if (fs1.ReadByte() != fs2.ReadByte())
                    return false;
            }
            
            return true;
        }

        [Fact]
        public async Task ExecuteIntegrationTest_RedactionShouldNotChangeOtherPartsOfFile()
        {
            string outputFile = "output2.binlog";
            File.Delete(outputFile);
            string inputFile = Path.Combine("assets", "console.binlog");

            File.Exists(inputFile).Should().BeTrue();

            BinlogRedactorOptions options = new BinlogRedactorOptions(new string[] { "restore", "console" })
            {
                InputPath = inputFile,
                OutputFileName = outputFile,
                OverWrite = false,
            };
            using FileDeletingScope fileDeletingScope = new FileDeletingScope(outputFile);
            BinlogRedactor binlogRedactor = new BinlogRedactor(_loggerFactory.CreateLogger<BinlogRedactor>(), new PhysicalFileSystem(), new SimpleBinlogProcessor());
            (await binlogRedactor.Execute(options).ConfigureAwait(false)).Should().Be(BinlogRedactorErrorCode.Success);

            File.Exists(outputFile).Should().BeTrue();

            // This is currently failing as the redaction is not deterministic - it produces slightly different output each time
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
