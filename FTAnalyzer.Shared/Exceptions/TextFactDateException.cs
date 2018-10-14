using System;

namespace FTAnalyzer
{
    [Serializable]
    class TextFactDateException : Exception
    {
        public TextFactDateException(string message)
            : base(message)
        { }
    }
}
