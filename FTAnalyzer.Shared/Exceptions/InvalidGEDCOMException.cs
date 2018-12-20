using System;

namespace FTAnalyzer
{
    [Serializable]
    public class InvalidGEDCOMException : Exception
    {
        public InvalidGEDCOMException(string message)
            : base(message)
        { }
    }
}
