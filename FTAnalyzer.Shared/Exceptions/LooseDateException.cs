using System;

namespace FTAnalyzer
{
    [Serializable]
    public class LooseDataException : Exception
    {
        public LooseDataException(string message)
            : base(message)
        { }

        public LooseDataException()
        {
        }

        public LooseDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
