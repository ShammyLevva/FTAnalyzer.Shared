using System;

namespace FTAnalyzer
{
    [Serializable]
    public class MissingPrimaryKeyException : Exception
    {
        public MissingPrimaryKeyException(string message) : base(message)
        {
        }

        public MissingPrimaryKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
