using System;
using System.Runtime.Serialization;

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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
