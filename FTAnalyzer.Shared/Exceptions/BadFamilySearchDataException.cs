using System;
using System.Runtime.Serialization;

namespace FTAnalyzer
{
    [Serializable]
    public class BadFamilySearchDataException : Exception
    {
        public BadFamilySearchDataException(string message)
            : base(message)
        { }

        public BadFamilySearchDataException()
        {
        }

        public BadFamilySearchDataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BadFamilySearchDataException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }
    }
}
