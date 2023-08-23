namespace Microsoft.Build.BinlogRedactor.BinaryLog;

public interface ISensitiveDataProcessor
{
    string ReplaceSensitiveData(string text);
    bool IsSensitiveData(string text);
}
