using FTAnalyzer.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class SurnameStats : IDisplaySurnames
    {
        public string Surname { get; private set; }
        public int Individuals { get; set; }
        public int Families { get; set; }
        public int Marriages { get; set; }
        public string GOONSpage { get; set; }

        public SurnameStats(string name)
        {
            Surname = name;
            Individuals = 0;
            Families = 0;
            Marriages = 0;
            GOONSpage = string.Empty;
        }

        public int CompareTo(IDisplaySurnames other) => string.Compare(Surname, other.Surname, StringComparison.Ordinal);

        public IComparer<IDisplaySurnames> GetComparer(string columnName, bool ascending)
        {
            switch (columnName)
            {
                case "Surname": return CompareComparableProperty<IDisplaySurnames>(f => f.Surname, ascending);
                case "Individuals": return CompareComparableProperty<IDisplaySurnames>(f => f.Individuals, ascending);
                case "Families": return CompareComparableProperty<IDisplaySurnames>(f => f.Families, ascending);
                case "Marriages": return CompareComparableProperty<IDisplaySurnames>(f => f.Marriages, ascending);
                default: return null;
            }
        }

        Comparer<T> CompareComparableProperty<T>(Func<IDisplaySurnames, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                var c1 = accessor(x as IDisplaySurnames);
                var c2 = accessor(y as IDisplaySurnames);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }

    public class SurnameStatsComparer : IEqualityComparer<IDisplaySurnames>
    {
        public bool Equals(IDisplaySurnames a, IDisplaySurnames b)
        {
            return a.Surname.ToUpper() == b.Surname.ToUpper() &&
                    a.Individuals == b.Individuals &&
                    a.Families == b.Families &&
                    a.Marriages == b.Marriages;
        }

        public int GetHashCode(IDisplaySurnames obj) => base.GetHashCode();
    }
}
