namespace FTAnalyzer
{
    public class LooseDeathComparer : Comparer<IDisplayLooseDeath>
    {
        public override int Compare(IDisplayLooseDeath? x, IDisplayLooseDeath? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            return x.Surname.Equals(y.Surname, StringComparison.OrdinalIgnoreCase)
                ? x.Forenames.Equals(y.Forenames, StringComparison.OrdinalIgnoreCase)
                    ? x.BirthDate.CompareTo(y.BirthDate)
                    : string.Compare(x.Forenames, y.Forenames, StringComparison.Ordinal)
                : string.Compare(x.Surname, y.Surname, StringComparison.Ordinal);
        }
    }
}
