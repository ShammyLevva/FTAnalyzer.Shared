using System;
using System.Runtime.Serialization;

namespace FTAnalyzer
{
    [Serializable]
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }

        public NotFoundException()
        {
        }

        public NotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }
    }
}
