namespace FTAnalyzer
{
    public class ColourCensusComparer : Comparer<IDisplayColourCensus>
    {
        public override int Compare(IDisplayColourCensus? x, IDisplayColourCensus? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return x.Surname == y.Surname
                ? x.Forenames == y.Forenames
                    ? x.BirthDate.CompareTo(y.BirthDate)
                    : string.Compare(x.Forenames, y.Forenames, StringComparison.Ordinal)
                : string.Compare(x.Surname, y.Surname, StringComparison.Ordinal);
        }
    }
}
