namespace FTAnalyzer
{
    class CensusIndividualComparer : IEqualityComparer<CensusIndividual>
    {
        public bool Equals(CensusIndividual? x, CensusIndividual? y)
        {
            if (x is null && y is null)
                return true;
            if (x is null || y is null)
                return false;
            return x.IndividualID.Equals(y.IndividualID);
        }

        public int GetHashCode(CensusIndividual obj)    
        {
            return obj?.IndividualID.GetHashCode() ?? 0;
        }
    }
}
