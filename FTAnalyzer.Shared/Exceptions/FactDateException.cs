using System;

namespace FTAnalyzer
{
    [Serializable]
    public class FactDateException : Exception
    {
        public FactDateException(string message)
            : base(message)
        { }
    }
}
