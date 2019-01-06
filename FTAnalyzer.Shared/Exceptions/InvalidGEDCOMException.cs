using System;

namespace FTAnalyzer
{
    [Serializable]
    public class InvalidGEDCOMException : Exception
    {
        string OriginalLine { get; }
        long LineNumber { get; }
        public InvalidGEDCOMException(string message, string line, long lineNumber)
            : base(message)
        {
            OriginalLine = line;
            LineNumber = lineNumber;
        }
    }
}
