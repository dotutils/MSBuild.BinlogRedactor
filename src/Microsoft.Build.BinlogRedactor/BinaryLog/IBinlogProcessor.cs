using Microsoft.Build.BinlogRedactor.Reporting;

namespace Microsoft.Build.BinlogRedactor.BinaryLog;

public interface IBinlogProcessor
{
    /// <summary>
    /// Passes all string data from the input file to the <see cref="ISensitiveDataProcessor"/> and writes the
    /// resulting binlog file so that only the appropriate textual data is altered, while result is still valid binlog.
    /// </summary>
    Task<BinlogRedactorErrorCode> ProcessBinlog(
        string inputFileName,
        string outputFileName,
        bool skipEmbeddedFiles,
        ISensitiveDataProcessor sensitiveDataProcessor,
        CancellationToken cancellationToken);
}
