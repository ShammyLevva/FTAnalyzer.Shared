using System;

namespace FTAnalyzer
{
    [Serializable]
    public class InvalidXMLFactException : Exception
    {
        public InvalidXMLFactException(string message)
            : base(message)
        { }

        public InvalidXMLFactException()
        {
        }

        public InvalidXMLFactException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
