namespace FTAnalyzer
{
    public class CensusIndividualNameComparer : DefaultCensusComparer
    {
        public override int Compare(CensusIndividual? x, CensusIndividual? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            int r = string.Compare(x.CensusSurname, y.CensusSurname, StringComparison.Ordinal);
            if (r == 0) r = base.Compare(x, y);
            return r;
        }
    }
}
