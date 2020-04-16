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

        public override void GetObjectData(SerializationInfo info, StreamingContext context) => base.GetObjectData(info, context);

        protected InvalidGEDCOMException(SerializationInfo serializationInfo, StreamingContext streamingContext)
             : base(serializationInfo, streamingContext)
        { }

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
