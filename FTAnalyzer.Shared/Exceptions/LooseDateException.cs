using System;

namespace FTAnalyzer
{
    [Serializable]
    public class LooseDataException : Exception
    {
        public LooseDataException(string message)
            : base(message)
        { }

        protected LooseDataException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        { }

        public LooseDataException()
        {
        }

        public LooseDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
