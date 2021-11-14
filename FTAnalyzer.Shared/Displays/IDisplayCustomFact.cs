using System;
using FTAnalyzer.Utilities;

namespace FTAnalyzer
{
    public interface IDisplayCustomFact : IComparable<IDisplayCustomFact>, IColumnComparer<IDisplayCustomFact>
    {
        [ColumnDetail("Custom Fact Name", 400)]
        string CustomFactName { get; }

        [ColumnDetail("Individual Count", 120)]
        int IndividualCount { get; }

        [ColumnDetail("Family Count", 120)]
        int FamilyCount { get; }

        [ColumnDetail("Ignore", 70, ColumnDetail.ColumnAlignment.Center, ColumnDetail.ColumnType.CheckBox)]
        bool Ignore { get; set; }
    }
}
