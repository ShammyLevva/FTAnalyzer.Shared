using System;

namespace FTAnalyzer
{
    [Serializable]
    public class InvalidDoubleDateException : Exception
    {
        public InvalidDoubleDateException(string message)
            : base(message)
        { }

        protected InvalidDoubleDateException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        { }

        public InvalidDoubleDateException()
        {
        }

        public InvalidDoubleDateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
