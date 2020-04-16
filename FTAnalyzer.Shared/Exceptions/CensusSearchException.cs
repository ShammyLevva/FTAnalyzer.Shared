using System;

namespace FTAnalyzer
{
    [Serializable]
    public class CensusSearchException : Exception
    {
        public CensusSearchException(string message)
            : base(message)
        { }

        protected CensusSearchException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        { }

        public CensusSearchException()
        {
        }

        public CensusSearchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
