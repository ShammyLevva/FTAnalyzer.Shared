using System;
using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplaySurnames : IComparable<IDisplaySurnames>, IColumnComparer<IDisplaySurnames>
    {
        [ColumnDetail("Surname", 400)]
        string Surname { get; }
        [ColumnDetail("Count", 70)]
        int Count { get; }
    }
}
