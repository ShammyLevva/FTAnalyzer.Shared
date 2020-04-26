using System;
using System.Runtime.Serialization;

namespace FTAnalyzer
{
    [Serializable]
    public class OpenDatabaseException : Exception
    {
        public OpenDatabaseException(string message)
            : base(message)
        { }

        public OpenDatabaseException()
        {
        }

        public OpenDatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected OpenDatabaseException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }
    }
}
