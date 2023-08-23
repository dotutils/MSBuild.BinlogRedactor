using Microsoft.Build.BinlogRedactor.Reporting;

namespace Microsoft.Build.BinlogRedactor.BinaryLog;

public interface IBinlogProcessor
{
    Task<BinlogRedactorErrorCode> ProcessBinlog(
        string inputFileName,
        string outputFileName,
        ISensitiveDataProcessor sensitiveDataProcessor,
        CancellationToken cancellationToken);
}