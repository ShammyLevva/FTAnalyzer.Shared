using System;

namespace FTAnalyzer.Utilities
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]

    public class ColumnWidth : Attribute
    {
        public float ColWidth { get; }

        public ColumnWidth(float width) => ColWidth = width;
    }
}
