using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DisplayCustomFact(string factname, int indcount, int famcount, bool ignore) : IDisplayCustomFact
    {
        public string CustomFactName { get; } = factname;
        public int IndividualCount { get; } = indcount;
        public int FamilyCount { get; } = famcount;
        public bool Ignore { get; set; } = ignore;

        public int CompareTo(IDisplayCustomFact? that) => string.Compare(CustomFactName, that.CustomFactName, System.StringComparison.Ordinal);

        public IComparer<IDisplayCustomFact> GetComparer(string columnName, bool ascending)
        {
            return columnName switch
            {
                "CustomFactName" => CompareComparableProperty<IDisplayCustomFact>(f => f.CustomFactName, ascending),
                "IndividualCount" => CompareComparableProperty<IDisplayCustomFact>(f => f.IndividualCount, ascending),
                "FamilyCount" => CompareComparableProperty<IDisplayCustomFact>(f => f.FamilyCount, ascending),
                "Ignore" => CompareComparableProperty<IDisplayCustomFact>(f => f.Ignore, ascending),
                _ => CompareComparableProperty<IDisplayCustomFact>(f => f.CustomFactName, ascending),
            };
        }

        static Comparer<T> CompareComparableProperty<T>(Func<IDisplayCustomFact, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                if(x is not IDisplayCustomFact facX)
                    return ascending ? 1 : -1;
                if (y is not IDisplayCustomFact facY)
                    return ascending ? 1 : -1;
                var c1 = accessor(facX);
                var c2 = accessor(facY);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }
}

