using System;

namespace FTAnalyzer.Shared.Utilities
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]

    public class ColumnWidth : Attribute
    {
        public int ColWidth { get; }

        public ColumnWidth(int width)
        {
            ColWidth = width;
        }
    }
}
