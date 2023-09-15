using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Build.BinlogRedactor.BinaryLog;
using Microsoft.Build.BinlogRedactor.IO;
using Microsoft.Build.BinlogRedactor.Reporting;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.PlatformAbstractions.Interfaces;
using VerifyTests;
using Xunit.Abstractions;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        [Fact]
        public void ReadInputBinlog()
        {
            string path = Path.Combine("assets", "console.binlog");
            ReadBinlog(path);
        }

        [Fact]
        public void ReadOutputBinlog()
        {
            //string path =
            //    @"C:\src\MSBuildBinlogRedactor\test\Microsoft.Build.BinlogRedactor.Tests\bin\Debug\net8.0\output.binlog";
            //ReadBinlog(path);

            ReadBinlog2(Path.Combine("assets", "console-fixed03.binlog"), null);
        }

        //"C:\src\MSBuildBinlogRedactor\test\Microsoft.Build.BinlogRedactor.Tests\bin\Debug\net8.0\output.binlog"

        public void ReadBinlog(string path)
        {
            BinaryLogReplayEventSource originalEventsSource = new BinaryLogReplayEventSource();
            BuildEventArgsReader originalBuildEventsReader =
                BinaryLogReplayEventSource.OpenBuildEventsReader(path);

            originalEventsSource.Replay(originalBuildEventsReader, CancellationToken.None);
        }

        [Fact]
        public void Read2BinlogsTst()
        {
            //Read2Binlogs(Path.Combine("assets", "console-preview.binlog"),
            //    @"C:\src\MSBuildBinlogRedactor\test\Microsoft.Build.BinlogRedactor.Tests\bin\Debug\net8.0\assets\output-preview.binlog");

            Read2Binlogs(Path.Combine("assets", "console-fixed01.binlog"),
                @"C:\src\MSBuildBinlogRedactor\test\Microsoft.Build.BinlogRedactor.Tests\bin\Debug\net8.0\assets\output-fixed01.binlog");
        }

        public void Read2Binlogs(string path1, string path2)
        {
            var waitable1 = new AutoResetEvent(false);
            var waitable2 = new AutoResetEvent(false);


            var res1 = new AutoResetEvent(false);
            var res2 = new AutoResetEvent(false);

            BuildEventArgs arg1 = null;
            BuildEventArgs arg2 = null;

            Task.Factory.StartNew(() =>
                ReadBinlog2(path1, (arg) =>
                {
                    waitable1.WaitOne();
                    arg1 = arg;
                    res1.Set();
                }), TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(() =>
                ReadBinlog2(path2, (arg) =>
                {
                    waitable2.WaitOne();
                    arg2 = arg;
                    res2.Set();
                }), TaskCreationOptions.LongRunning);

            int eventsCount = 0;
            while (eventsCount < 3465)
            {
                waitable1.Set();
                waitable2.Set();

                res1.WaitOne();
                res2.WaitOne();

                if (arg1 == null || arg2 == null)
                {
                    //eof
                    break;
                }

                eventsCount++;

                if (arg1.GetType() != arg2.GetType())
                {

                    throw new Exception("Types are different");
                }

            }
        }

        // https://github.com/dotnet/templating/blob/7a437b5e79899092000d1fcf72376ed658bb8366/src/Microsoft.TemplateEngine.Core/Util/StreamProxy.cs#L13
        private class MyStreamProxy : Stream
        {
            private static int _instanceCount;
            private int _instanceId = Interlocked.Increment(ref _instanceCount) - 1;
            private static byte[][] _buffers = new byte[2][] { new byte[700000], new byte[700000] };

            private readonly Stream _sourceStream;
            // private readonly Stream? _targetStream;

            public MyStreamProxy(Stream sourceStream)
            {
                _sourceStream = sourceStream;
            }

            public override bool CanRead => _sourceStream.CanRead;

            public override bool CanSeek => _sourceStream.CanSeek;

            public override bool CanWrite => _sourceStream.CanWrite;

            public override long Length => _sourceStream.Length;

            public override long Position
            {
                get => _sourceStream.Position;

                set
                {
                    UnexpectedCall();
                    _sourceStream.Position = value;
                }
            }

            public override void Flush() => _sourceStream.Flush();

            public override long Seek(long offset, SeekOrigin origin) => _sourceStream.Seek(offset, origin);

            public override void SetLength(long value)
            {
                UnexpectedCall();
                _sourceStream.SetLength(value);
            }

            int readBytes = 0;
            private static object _locker = new object();
            private static int _readsMade = 0;
            private static int[] _offsets = new int[2];
            public override int Read(byte[] buffer, int offset, int count)
            {
                var res = _sourceStream.Read(buffer, offset, count);
                Buffer.BlockCopy(buffer, 0, _buffers[_instanceId], readBytes, res);
                _offsets[_instanceId] += res;

                //lock (_locker)
                //{
                //    _readsMade++;
                //    if (_readsMade % 2 == 0 && readBytes + res >= /*24643*/143325)
                //    {
                //        for (int i = 0; i <= readBytes + res; i++)
                //        {
                //            if (_buffers[0][i] != _buffers[1][i])
                //            {
                //                //throw new Exception("test1");
                //                Console.WriteLine("test1");
                //            }
                //        }
                //    }
                //}

                readBytes += res;
                if (readBytes >= /*24643*/198096)
                {
                    //throw new Exception("test");
                }
                return res;
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                UnexpectedCall();
                _sourceStream.Write(buffer, offset, count);
            }

            private void UnexpectedCall([CallerMemberName] string? caller = null)
            {
                throw new InvalidOperationException($"Unexpected call to {caller}");
            }

            public override void Close()
            {
                _sourceStream.Close();
                base.Close();
            }
        }

        private void ReadBinlog2(string path, Action<BuildEventArgs>? onArg)
        {
            BinaryLogReplayEventSource s = new BinaryLogReplayEventSource();
            BinaryReader br;
            Stream? stream = null;
            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                //var wrapStream = new MyStreamProxy(stream);
                //var gzipStream = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: false);
                var wrapStream = new MyStreamProxy(stream);


                // wrapping the GZipStream in a buffered stream significantly improves performance
                // and the max throughput is reached with a 32K buffer. See details here:
                // https://github.com/dotnet/runtime/issues/39233#issuecomment-745598847
                //var bufferedStream = new BufferedStream(gzipStream, 32768);
                br = new BinaryReader(wrapStream);
            }
            catch (Exception)
            {
                stream?.Dispose();
                throw;
            }

            s.AnyEventRaised += (sender, args) => { onArg?.Invoke(args); };

            s.Replay(br, true, CancellationToken.None);
        }

        [Fact]
        public void ReZipTest()
        {
            ZipArchive inArchive = ZipFile.OpenRead(@"C:\tmp\trash\compare\orig.zip");

            using var fileStream = new FileStream(@"C:\tmp\trash\compare\orig-replayed05.zip", FileMode.Create, FileAccess.ReadWrite, FileShare.Delete);
            using var outArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);

            //Encoding noBom = new UTF8Encoding(false);

            int i = 0;
            foreach (var entry in inArchive.Entries.OrderBy(e => e.LastWriteTime))
            {
                i++;
                var archiveEntry = outArchive.CreateEntry(entry.FullName);
                archiveEntry.LastWriteTime = entry.LastWriteTime;
                using var outStr = archiveEntry.Open();
                using var intStr = entry.Open();

                //using var _contentReader = new StreamReader(intStr, new System.Text.UTF8Encoding(false));
                //var content = _contentReader.ReadToEnd();
                //using var writer = new StreamWriter(outStr, _contentReader.CurrentEncoding);
                //writer.Write(content);

                //using var _contentReader = new StreamReader(intStr, new System.Text.UTF8Encoding(false));
                //var content = _contentReader.ReadToEnd();
                //using var writer = new StreamWriter(outStr);

                /*************** WORKS ****************/

                //byte[] tmp = new byte[Encoding.UTF8.Preamble.Length];
                //bool hasBom = intStr.Read(tmp) == tmp.Length && Encoding.UTF8.Preamble.SequenceEqual(tmp);
                ///*intStr.Position = 0;*/
                //intStr.Dispose();
                //intStr = entry.Open();
                using var rdr = new StreamReader(intStr, /*Encoding.UTF8*/new UTF8Encoding(false));

                string str = rdr.ReadToEnd();

                //if (hasBom != !(Equals(rdr.CurrentEncoding, noBom)))
                //{
                //    throw new Exception("test encoding " + i);
                //}


                //byte[] rs = Encoding.UTF8.GetBytes(str);
                //if (hasBom)
                //{
                //    rs = Encoding.UTF8.GetPreamble().Concat(rs).ToArray();
                //}

                using MemoryStream ms = new MemoryStream();

                using StreamWriter sw = new StreamWriter(ms, rdr.CurrentEncoding);
                sw.Write(str);
                sw.Flush();

                //byte[] rs2 = ms.ToArray();

                //if (!rs.SequenceEqual(rs2))
                //{
                //    throw new Exception("test" + i);
                //}

                //MemoryStream mout = new MemoryStream(ms.ToArray());
                ms.Position = 0;

                //MemoryStream ms2 = new MemoryStream(rdr.CurrentEncoding.GetBytes(str));
                //byte[] encodB = ms2.ToArray();

                //MemoryStream ms3 = new MemoryStream();

                //StreamWriter sw2 = new StreamWriter(ms3, /*hasBom ? Encoding.UTF8 : noBom*/ rdr.CurrentEncoding);
                //sw2.Write(str);
                //sw2.Flush();

                //byte[] wrtrB = ms3.ToArray();

                //if (!encodB.SequenceEqual(wrtrB))
                //{
                //    throw new Exception("test" + i);
                //}


                ms.CopyTo(outStr);


                //intStr.Dispose();
                //outStr.Dispose();

                /*************** END WORKS ****************/

                //using var _contentReader = new StreamReader(intStr, new System.Text.UTF8Encoding(false));
                //var content = _contentReader.ReadToEnd();

                //var intStr2 = entry.Open();


                //MemoryStream ms2 = new MemoryStream();
                //using var writer = new StreamWriter(ms2, _contentReader.CurrentEncoding);
                //writer.Write(content);
                //byte[] res2 = ms2.ToArray();




                //MemoryStream ms = new MemoryStream();
                //intStr.CopyTo(ms);
                //byte[] res = ms.ToArray();

                //using var intStr2 = entry.Open();

                //using var _contentReader = new StreamReader(intStr2, noBom);
                //var content = _contentReader.ReadToEnd();
                //MemoryStream ms2 = new MemoryStream();
                //using var writer = new StreamWriter(ms2, _contentReader.CurrentEncoding);
                //writer.Write(content);
                //byte[] res2 = ms2.ToArray();

                //string s2 = Encoding.UTF8.GetString(res2);
                //byte[] res3 = Encoding.UTF8.GetBytes(content);
                //System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding(true);
                //byte[] res4 = utf8.GetBytes(content);

                //if (!res3.SequenceEqual(res))
                //{
                //    for (int j = 0; j < Math.Min(res.Length, res2.Length); j++)
                //    {
                //        if (res[j] != res2[j])
                //            throw new Exception("test-" + i + "-" + j);
                //    }

                //    throw new Exception("test" + i);
                //}


                //ms.Position = 0;
                //ms.CopyTo(outStr);
                ////writer.Write(res);

                //// intStr.CopyTo(outStr);
            }

            outArchive.Dispose();
            fileStream.Dispose();

            FilesAreBinaryEqual(new FileInfo(@"C:\tmp\trash\compare\orig.zip"), new FileInfo(@"C:\tmp\trash\compare\orig-replayed05.zip")).Should().BeTrue();
        }

        [Fact]
        public void CmpTest()
        {
            FilesAreBinaryEqual(new FileInfo(@"C:\tmp\trash\compare\orig.zip"), new FileInfo(@"C:\tmp\trash\orig.zip")).Should().BeTrue();
        }

        [Fact]
        public void ReZipTest2()
        {
            string dir = @"C:\tmp\trash\compare\orig";

            using var fileStream = new FileStream(@"C:\tmp\trash\compare\orig-redo06.zip", FileMode.Create, FileAccess.ReadWrite, FileShare.Delete);
            using var outArchive = new ZipArchive(fileStream, ZipArchiveMode.Create);

            foreach (string f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).OrderBy(File.GetLastWriteTime))
            {
                using FileStream fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.Read);
                //var entry = outArchive.CreateEntryFromFile(f, f.Replace(dir, ""));
                var entry = outArchive.CreateEntry(f.Substring(dir.Length + 1));
                entry.LastWriteTime = File.GetLastWriteTime(f);
                using var entryStream = entry.Open();
                fs.CopyTo(entryStream);
            }

            outArchive.Dispose();
            fileStream.Dispose();

            FilesAreBinaryEqual(new FileInfo(@"C:\tmp\trash\compare\orig.zip"), new FileInfo(@"C:\tmp\trash\compare\orig-redo06.zip")).Should().BeTrue();
        }

        [Fact]
        public void ZipBomTest()
        {
            ZipArchive archive = ZipFile.OpenRead(@"C:\tmp\trash\compare\orig01.zip");

            string fileName = "project.assets.json";
            //"project.assets.json";
            //"roslyn\\microsoft.csharp.core.targets";
            var entry = archive.Entries.First(e => e.FullName.EndsWith(fileName, StringComparison.InvariantCultureIgnoreCase));

            Stream stream = entry.Open();

            byte[] bytes = ReadFully(stream);
            string s = ConvertFromUtf8(bytes);

            stream.Close();
            stream = entry.Open();

            using StreamReader reader = new StreamReader(stream, new System.Text.UTF8Encoding(false));
            string content = reader.ReadToEnd();
            int i = (int)content[0];

            System.Text.UTF8Encoding utf8 = new System.Text.UTF8Encoding(false);

            byte[] bytes2 = new System.Text.UTF8Encoding(true).GetBytes(content);
            byte[] bytes3 = new System.Text.UTF8Encoding(false).GetBytes(content);
            byte[] bytes4 = reader.CurrentEncoding.GetBytes(content);

            Console.WriteLine(i);
            Console.WriteLine(content);

            MemoryStream ms = new MemoryStream();

            StreamWriter sw = new StreamWriter(ms, System.Text.Encoding.UTF8);
            sw.Write(content);

            byte[] res = ms.ToArray();


            //
            // This is the way
            //

            MemoryStream ms1 = new MemoryStream();

            StreamWriter sw1 = new StreamWriter(ms1, reader.CurrentEncoding);
            sw1.Write(content);

            byte[] res1 = ms1.ToArray();
        }

        public string ConvertFromUtf8(byte[] bytes)
        {
            var enc = new UTF8Encoding(true);
            var preamble = enc.GetPreamble();
            if (preamble.Where((p, i) => p != bytes[i]).Any())
                throw new ArgumentException("Not utf8-BOM");
            return enc.GetString(bytes.Skip(preamble.Length).ToArray());
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        [Fact]
        public async Task ExecuteIntegrationTest_NoOpRedactionShouldNotChangeFile()
        {
            Environment.SetEnvironmentVariable("MSBUILDDETERMNISTICBINLOG", "1");

            string outputFile = /*"cons-compr-unbuffered.binlog"*/"out-orchard-fin.binlog";
            string inputFile = Path.Combine("assets", /*"cons-compr-unbuffered.binlog"*/"orchard-fin.binlog");

            //inputFile = outputFile;
            //outputFile = "out-prev-v-twicereplayed.binlog";

            File.Delete(outputFile);

            File.Exists(inputFile).Should().BeTrue();

            BinlogRedactorOptions options = new BinlogRedactorOptions(new string[] { Guid.NewGuid().ToString() })
            {
                InputPath = inputFile, OutputFileName = outputFile, OverWrite = false,
            };
            // Will delete file at the end of function
            //using FileDeletingScope fileDeletingScope = new FileDeletingScope(outputFile);
            BinlogRedactor binlogRedactor = new BinlogRedactor(_loggerFactory.CreateLogger<BinlogRedactor>(),
                new PhysicalFileSystem(), new SimpleBinlogProcessor());
            try
            {
                (await binlogRedactor.Execute(options).ConfigureAwait(false)).Should()
                    .Be(BinlogRedactorErrorCode.Success);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Assert.Fail(e.ToString());
            }

            File.Exists(outputFile).Should().BeTrue();

            // This is currently failing as the redaction does not preserve content of the original file
            FilesAreBinaryEqual(new FileInfo(inputFile), new FileInfo(outputFile)).Should().BeTrue();
        }

        [Fact]
        public void CompareCompressions()
        {
            string inputFile = Path.Combine("assets", "console-uncompressed.binlog");
            string outputFile1 = "console-uncompressed.binlog.gz1";
            string outputFile2 = "console-uncompressed.binlog.gz2";

            for (int i = 0; i < 20; i++)
            {
                outputFile1 = $"console-uncompressed-{i}.binlog.gz1";
                outputFile2 = $"console-uncompressed-{i}.binlog.gz2";

                CopyInputToOutput(inputFile, outputFile1, 800);
                CopyInputToOutput(inputFile, outputFile2, 905);

                File.Exists(outputFile1).Should().BeTrue();
                File.Exists(outputFile2).Should().BeTrue();

                FilesAreBinaryEqual(new FileInfo(outputFile1), new FileInfo(outputFile2)).Should().BeTrue();
                // FilesAreBinaryEqual(new FileInfo(inputFile), new FileInfo(outputFile2)).Should().BeTrue();
            }
        }

        static Random rnd = new Random();
        private static void CopyInputToOutput(string inputPath, string outputPath, int blockSize)
        {
            File.Delete(outputPath);

            using Stream inStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            // inStream = new GZipStream(inStream, CompressionMode.Decompress, leaveOpen: false);
            var inBinaryWriter = new BinaryReader(inStream);

            Stream outStream = new FileStream(outputPath, FileMode.Create);
            outStream = new GZipStream(outStream, CompressionLevel.Optimal);
            outStream = new /*BufferedStream*/MyBufferedStream(outStream, 32768);
            var outBinaryWriter = new BinaryWriter(outStream);

            //int blockSize = 80;
            byte[] res;
            while ((res = inBinaryWriter.ReadBytes(blockSize)).Length > 0)
            {
                blockSize = rnd.Next(blockSize, 1000);
                outBinaryWriter.Write(res);
            }

            outStream.Close();
        }

        public class MyBufferedStream : Stream
        {
            readonly Stream _stream;
            readonly byte[] _buffer;
            int _position;

            public MyBufferedStream(Stream stream, int size = 1024)
            {
                _stream = stream;
                _buffer = new byte[size];
            }

            public override void Flush()
            {
                _stream.Write(_buffer, 0, _position);
                _position = 0;
            }

            public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

            public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

            public override void SetLength(long value) => throw new NotImplementedException();

            public override void Write(byte[] buffer, int offset, int count)
            {
                int srcOffset = offset;
                do
                {
                    int currentCount = Math.Min(count, _buffer.Length - _position);
                    Buffer.BlockCopy(buffer, srcOffset, _buffer, _position, currentCount);
                    _position += currentCount;
                    count -= currentCount;
                    srcOffset += currentCount;

                    if (_position == _buffer.Length)
                    {
                        Flush();
                    }
                } while (count > 0);
            }

            public override bool CanRead => _stream.CanRead;
            public override bool CanSeek => _stream.CanSeek;
            public override bool CanWrite => _stream.CanWrite;
            public override long Length => _stream.Length;

            public override long Position
            {
                get => _stream.Position + _position;
                set => throw new NotImplementedException("Cannot set position");
            }

            public override void Close()
            {
                Flush();
                _stream.Close();
                base.Close();
            }
        }

        private static bool FilesAreBinaryEqual(FileInfo first, FileInfo second)
        {
            //if (first.Length != second.Length)
            //    return false;
            
            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            using FileStream fs1 = first.OpenRead();
            using FileStream fs2 = second.OpenRead();
            for (int i = 0; i < first.Length; i++)
            {
                byte b1 = (byte)fs1.ReadByte();
                byte b2 = (byte)fs2.ReadByte();
                if (b1 != b2)
                    Assert.Fail($"Files ({first.Name}:{first.Length} and {second.Name}:{second.Length} sizes) are not equal at byte {i} ({b1} vs {b2})");
                    //return false;
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
                InputPath = inputFile, OutputFileName = outputFile, OverWrite = false,
            };
            using FileDeletingScope fileDeletingScope = new FileDeletingScope(outputFile);
            BinlogRedactor binlogRedactor = new BinlogRedactor(_loggerFactory.CreateLogger<BinlogRedactor>(),
                new PhysicalFileSystem(), new SimpleBinlogProcessor());
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
