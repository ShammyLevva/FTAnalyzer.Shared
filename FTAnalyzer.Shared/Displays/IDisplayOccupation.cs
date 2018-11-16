using System;
using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayOccupation : IComparable<IDisplayOccupation>
    {
        [ColumnDetail("Occupation", 400)]
        string Occupation { get; }
        [ColumnDetail("Count", 70, ColumnDetail.ColumnAlignment.Right)]
        int Count { get; }
    }
}
