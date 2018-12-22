using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplaySource
    {
        [ColumnDetail("Source Ref", 100)]
        string SourceID { get; }
        [ColumnDetail("Source Title", 500)]
        string SourceTitle { get; }
        [ColumnDetail("Publication", 200)]
        string Publication { get; }
        [ColumnDetail("Author", 200)]
        string Author { get; }
        [ColumnDetail("Source Text", 300)]
        string SourceText { get; }
        [ColumnDetail("Source Medium", 200)]
        string SourceMedium { get; }
        [ColumnDetail("Num Facts", 60, ColumnDetail.ColumnAlignment.Right)]
        int FactCount { get; }
    }
}
