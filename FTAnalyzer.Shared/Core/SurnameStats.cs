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
                "Surname" => CompareComparableProperty(f => f.Surname, ascending),
                "Individuals" => CompareComparableProperty(f => f.Individuals, ascending),
                "Families" => CompareComparableProperty(f => f.Families, ascending),
                "Marriages" => CompareComparableProperty(f => f.Marriages, ascending),
                _ => CompareComparableProperty(f => f.Surname, ascending),
            };
        }

        static IComparer<IDisplaySurnames> CompareComparableProperty(
            Func<IDisplaySurnames, IComparable> accessor,
            bool ascending)
        {
            return Comparer<IDisplaySurnames>.Create((x, y) =>
            {
                if (x is null && y is null) return 0;
                if (x is null) return ascending ? -1 : 1;
                if (y is null) return ascending ? 1 : -1;

                var c1 = accessor(x);
                var c2 = accessor(y);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }

    public sealed class SurnameStatsComparer : IEqualityComparer<IDisplaySurnames>
    {
        public bool Equals(IDisplaySurnames? a, IDisplaySurnames? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;

            return string.Equals(a.Surname, b.Surname, StringComparison.CurrentCultureIgnoreCase)
                   && a.Individuals == b.Individuals
                   && a.Families == b.Families
                   && a.Marriages == b.Marriages;
        }

        public int GetHashCode(IDisplaySurnames obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            return HashCode.Combine(
                obj.Surname?.ToUpperInvariant(),
                obj.Individuals,
                obj.Families,
                obj.Marriages);
        }
    }
}
