namespace FTAnalyzer
{
    public class CensusFamilyGedComparer : Comparer<CensusIndividual>
    {
        public override int Compare(CensusIndividual? x, CensusIndividual? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            int r = string.Compare(x.FamilyID, y.FamilyID, StringComparison.Ordinal);
            if (r == 0)
            {
                r = x.Position - y.Position;
            }
            return r;
        }
    }
}
