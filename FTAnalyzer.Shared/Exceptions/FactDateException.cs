using System;

namespace FTAnalyzer
{
    [Serializable]
    public class FactDateException : Exception
    {
        public FactDateException(string message)
            : base(message)
        { }

        public FactDateException()
        {
        }

        public FactDateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
