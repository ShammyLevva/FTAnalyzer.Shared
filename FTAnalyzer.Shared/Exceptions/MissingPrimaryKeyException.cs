using System;
using System.Runtime.Serialization;

namespace FTAnalyzer
{
    [Serializable]
    public class MissingPrimaryKeyException : Exception
    {
        public StreamingContext StreamingContext { get; }

        public MissingPrimaryKeyException(string message) : base(message)
        {
        }

        public MissingPrimaryKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public MissingPrimaryKeyException()
        {
        }
        protected MissingPrimaryKeyException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
            StreamingContext = streamingContext;
        }
    }
}
