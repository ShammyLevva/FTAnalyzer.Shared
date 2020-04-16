using System;

namespace FTAnalyzer
{
    [Serializable]
    public class FactDateException : Exception
    {
        public FactDateException(string message)
            : base(message)
        { }

        protected FactDateException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        { }

        public FactDateException()
        {
        }

        public FactDateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
