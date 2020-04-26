using System;
using System.Runtime.Serialization;

namespace FTAnalyzer
{
    [Serializable]
    public class TextFactDateException : Exception
    {
        public TextFactDateException(string message)
            : base(message)
        { }

        public TextFactDateException()
        {
        }

        public TextFactDateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TextFactDateException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }
    }
}
