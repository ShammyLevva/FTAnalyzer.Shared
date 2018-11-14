using System;
#if __MACOS__
using AppKit;
#endif

namespace FTAnalyzer.Utilities
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]

    public class ColumnDetail : Attribute
    {
        public string ColumnName { get; }
        public float ColumnWidth { get; }
#if __PC__
        public AlignmentclassforPC Alignment { get; }
#elif __MACOS__
        public NSTextAlignment Alignment { get; }
#endif
        public ColumnDetail(string name, float width) : this(name, width, ColumnAlignment.Left) { }
        public ColumnDetail(string name, float width, ColumnAlignment alignment)
        {
            ColumnName = name;
            ColumnWidth = width;
#if __PC__
            switch(alignment)
            {
                case ColumnAlignment.Left : Alignment = AlignmentclassforPC.Left; break;
                case ColumnAlignment.Right: Alignment = AlignmentclassforPC.Left; break;
                case ColumnAlignment.Center: Alignment = AlignmentclassforPC.Left; break;
            }
#elif __MACOS__
            switch (alignment)
            {
                case ColumnAlignment.Left : Alignment = NSTextAlignment.Left; break;
                case ColumnAlignment.Right: Alignment = NSTextAlignment.Right; break;
                case ColumnAlignment.Center: Alignment = NSTextAlignment.Center; break;
            }
#endif

        }

        public enum ColumnAlignment { Left, Right, Center };
    }
}
