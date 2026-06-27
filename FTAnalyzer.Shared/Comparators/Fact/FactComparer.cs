namespace FTAnalyzer
{
    public class FactComparer : IEqualityComparer<Fact>, IComparer<IDisplayFact>
    {
        public bool Equals(Fact? x, Fact? y) => x is not null && y is not null && x.FactType == y.FactType && x.FactDate == y.FactDate && x.Location == y.Location;

        public int GetHashCode(Fact obj) => IntDate(obj.FactDate.StartDate) * 10 + IntDate(obj.FactDate.EndDate);

        static int IntDate(DateTime date) => (date.Year * 100 + date.Month) * 100 + date.Day;

        public int Compare(IDisplayFact? x, IDisplayFact? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            int result = string.Compare(x.TypeOfFact, y.TypeOfFact, StringComparison.OrdinalIgnoreCase);
            if (result != 0) return result;

            result = x.FactDate.CompareTo(y.FactDate);
            if (result != 0) return result;

            return x.Location.CompareTo(y.Location);
        }
    }
}
