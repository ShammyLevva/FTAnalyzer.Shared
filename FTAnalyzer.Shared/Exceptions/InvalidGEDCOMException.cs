﻿using System.Runtime.Serialization;

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

        public InvalidGEDCOMException()
        {
        }

        public InvalidGEDCOMException(string message) : base(message)
        {
        }

        public InvalidGEDCOMException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
