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
#endif
        public ColumnDetail(string name, float width, ColumnAlignment alignment = ColumnAlignment.Left, ColumnType columnType = ColumnType.TextBox)
        {
            ColumnName = name;
            ColumnWidth = width;
#if __PC__
            TypeofColumn = columnType;
            switch (alignment)
            {
                case ColumnAlignment.Left: Alignment = DataGridViewContentAlignment.MiddleLeft; break;
                case ColumnAlignment.Right: Alignment = DataGridViewContentAlignment.MiddleRight; break;
                case ColumnAlignment.Center: Alignment = DataGridViewContentAlignment.MiddleCenter; break;
            }
#endif
        }
        public enum ColumnAlignment { Left, Right, Center };

        public enum ColumnType { TextBox, LinkCell, CheckBox, Icon };
    }
}
