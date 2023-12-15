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
                _ => CompareComparableProperty<IDisplayOccupation>(f => f.Occupation, ascending),
            };
        }

        static Comparer<T> CompareComparableProperty<T>(Func<IDisplayOccupation, IComparable> accessor, bool ascending)
        {
            return Comparer<T>.Create((x, y) =>
            {
                if (x is not IDisplayOccupation occX)
                    return ascending ? 1 : -1;
                if (x is not IDisplayOccupation occY)
                    return ascending ? 1 : -1;
                var c1 = accessor(occX);
                var c2 = accessor(occY);
                var result = c1.CompareTo(c2);
                return ascending ? result : -result;
            });
        }
    }
}

