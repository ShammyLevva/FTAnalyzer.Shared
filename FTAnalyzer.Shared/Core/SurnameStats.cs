namespace FTAnalyzer
{
    public class SurnameStats(string name) : IDisplaySurnames
    {
        public string Surname { get; private set; } = name;
        public int Individuals { get; set; } = 0;
        public int Families { get; set; } = 0;
        public int Marriages { get; set; } = 0;
        public string GOONSpage { get; set; } = string.Empty;

        public int CompareTo(IDisplaySurnames? other) => string.Compare(Surname, other.Surname, StringComparison.Ordinal);

        public IComparer<IDisplaySurnames> GetComparer(string columnName, bool ascending)
        {
            return columnName switch
            {
                "Surname" => CompareComparableProperty<IDisplaySurnames>(f => f.Surname, ascending),
                "Individuals" => CompareComparableProperty<IDisplaySurnames>(f => f.Individuals, ascending),
                "Families" => CompareComparableProperty<IDisplaySurnames>(f => f.Families, ascending),
                "Marriages" => CompareComparableProperty<IDisplaySurnames>(f => f.Marriages, ascending),
                _ => CompareComparableProperty<IDisplaySurnames>(f => f.Surname, ascending),
            };
        }

        static Comparer<T> CompareComparableProperty<T>(Func<IDisplaySurnames, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                if (x is not IDisplaySurnames surX)
                    return ascending ? 1 : -1;
                if (x is not IDisplaySurnames surY)
                    return ascending ? 1 : -1;
                var c1 = accessor(surX);
                var c2 = accessor(surY);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }

    public class SurnameStatsComparer : IEqualityComparer<IDisplaySurnames>
    {
        public bool Equals(IDisplaySurnames? a, IDisplaySurnames? b)
        {
            return a.Surname.Equals(b.Surname, StringComparison.CurrentCultureIgnoreCase) &&
                    a.Individuals == b.Individuals &&
                    a.Families == b.Families &&
                    a.Marriages == b.Marriages;
        }

        public int GetHashCode(IDisplaySurnames obj) => base.GetHashCode();
    }
}
