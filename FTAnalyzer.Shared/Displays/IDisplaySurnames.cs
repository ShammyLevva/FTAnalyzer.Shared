using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplaySurnames : IComparable<IDisplaySurnames>
    {
        [ColumnDetail("Surname", 200, ColumnDetail.ColumnAlignment.Left, ColumnDetail.ColumnType.LinkCell)]
        string Surname { get; }
        [ColumnDetail("Individuals", 175)]
        int Individuals { get; }
        [ColumnDetail("Families", 175)]
        int Families { get; }
        [ColumnDetail("Marriages", 175)]
        int Marriages { get; }

        Comparer<IDisplaySurnames> GetComparer(string columnName, bool ascending);
    }
}