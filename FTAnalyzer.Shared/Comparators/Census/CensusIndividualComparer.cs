namespace FTAnalyzer
{
    public class CensusIndividualComparer : IEqualityComparer<CensusIndividual>
    {
        public bool Equals(CensusIndividual? x, CensusIndividual? y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            return x.IndividualID.Equals(y.IndividualID, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(CensusIndividual obj)    
        {
            return obj?.IndividualID.GetHashCode(StringComparison.OrdinalIgnoreCase) ?? 0;
        }
    }
}
