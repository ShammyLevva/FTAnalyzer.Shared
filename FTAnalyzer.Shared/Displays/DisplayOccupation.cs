using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DisplayOccupation : IDisplayOccupation
    {
        public string Occupation { get; }
        public int Count { get; }

        public DisplayOccupation(string occupation,int count)
        {
            Occupation = occupation;
            Count = count;
        }

        public int CompareTo(IDisplayOccupation? that) => string.Compare(Occupation, that.Occupation, StringComparison.Ordinal);

        public IComparer<IDisplayOccupation> GetComparer(string columnName, bool ascending)
        {
            return columnName switch
            {
                "Occupation" => CompareComparableProperty<IDisplayOccupation>(f => f.Occupation, ascending),
                "Count" => CompareComparableProperty<IDisplayOccupation>(f => f.Count, ascending),
                _ => null,
            };
        }

        static Comparer<T> CompareComparableProperty<T>(Func<IDisplayOccupation, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var c1 = accessor(x as IDisplayOccupation);
                var c2 = accessor(y as IDisplayOccupation);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }
}

