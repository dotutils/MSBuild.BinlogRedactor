namespace Microsoft.Build.BinlogRedactor.BinaryLog;

public interface ISensitiveDataProcessor
{
    /// <summary>
    /// Processes the given text and if needed, replaces sensitive data with a placeholder.
    /// </summary>
    string ReplaceSensitiveData(string text);
    bool IsSensitiveData(string text);
}
