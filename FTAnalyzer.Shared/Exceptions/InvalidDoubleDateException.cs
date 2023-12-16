using System;

namespace FTAnalyzer
{
    [Serializable]
    public class InvalidDoubleDateException : Exception
    {
        public InvalidDoubleDateException(string message)
            : base(message)
        { }

        public InvalidDoubleDateException()
        {
        }

        public InvalidDoubleDateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
