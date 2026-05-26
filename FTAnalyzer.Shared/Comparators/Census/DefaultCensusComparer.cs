namespace FTAnalyzer
{
    public class DefaultCensusComparer : Comparer<CensusIndividual>
    {
        public override int Compare(CensusIndividual? x, CensusIndividual? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            int result = string.Compare(x.FamilyID, y.FamilyID, StringComparison.Ordinal);
            if (result == 0) result = x.Position - y.Position;
            return result;
        }
    }
}
