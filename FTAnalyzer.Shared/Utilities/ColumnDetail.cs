using System;
#if __PC__
using System.Windows.Forms;
#elif __MACOS__
using AppKit;
#endif

namespace FTAnalyzer.Utilities
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]

    public sealed class ColumnDetail : Attribute
    {
        public string ColumnName { get; }
        public float ColumnWidth { get; }
#if __PC__
        public DataGridViewContentAlignment Alignment { get; }
        public ColumnType TypeofColumn { get; }
#elif __MACOS__
        public NSTextAlignment Alignment { get; }
#endif
        public ColumnDetail(string name, float width, ColumnAlignment alignment = ColumnAlignment.Left, ColumnType columnType = ColumnType.TextBox)
        {
            ColumnName = name;
            ColumnWidth = width;
            TypeofColumn = columnType;
#if __PC__
            switch(alignment)
            {
                case ColumnAlignment.Left : Alignment = DataGridViewContentAlignment.MiddleLeft; break;
                case ColumnAlignment.Right: Alignment = DataGridViewContentAlignment.MiddleRight; break;
                case ColumnAlignment.Center: Alignment = DataGridViewContentAlignment.MiddleCenter; break;
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

        public enum ColumnType { TextBox, LinkCell, CheckBox, Icon };
    }
}
