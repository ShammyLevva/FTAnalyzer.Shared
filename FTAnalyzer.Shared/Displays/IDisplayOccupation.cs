using System;
using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayOccupation : IComparable<IDisplayOccupation>
    {
        [ColumnDetail("Occupation", 300)]
        string Occupation { get; }
        [ColumnDetail("Count", 50, ColumnDetail.ColumnAlignment.Right)]
        int Count { get; }
    }
}
