using System;

namespace FTAnalyzer
{
    [Serializable]
    class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
