using System;
using System.Runtime.Serialization;

namespace FTAnalyzer
{
    [Serializable]
    public class InvalidXMLFactException : Exception
    {
        public InvalidXMLFactException(string message)
            : base(message)
        { }

        public InvalidXMLFactException()
        {
        }

        public InvalidXMLFactException(string message, Exception innerException) : base(message, innerException)
        {
        }
        protected InvalidXMLFactException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }
    }
}
