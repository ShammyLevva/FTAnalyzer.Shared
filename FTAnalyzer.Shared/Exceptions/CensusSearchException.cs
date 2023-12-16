using System;

namespace FTAnalyzer
{
    [Serializable]
    public class CensusSearchException : Exception
    {
        public CensusSearchException(string message)
            : base(message)
        { }

        public CensusSearchException()
        {
        }

        public CensusSearchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
