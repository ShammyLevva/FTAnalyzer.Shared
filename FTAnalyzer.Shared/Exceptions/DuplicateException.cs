using System;
using System.Runtime.Serialization;

namespace FTAnalyzer
{
    [Serializable]
    public class DuplicateException : Exception
    {
        public DuplicateException(string message) : base(message)
        {
        }

        public DuplicateException(string message, Exception innerException) : base(message, innerException)
        {
        }
       public DuplicateException()
        {
        }

        protected DuplicateException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }
    }
}
