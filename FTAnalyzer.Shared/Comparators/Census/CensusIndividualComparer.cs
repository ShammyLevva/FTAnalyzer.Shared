using System.Collections.Generic;

namespace FTAnalyzer
{
    class CensusIndividualComparer : IEqualityComparer<CensusIndividual>
    {
        public bool Equals(CensusIndividual? x, CensusIndividual? y)
        {
            return x.IndividualID.Equals(y.IndividualID);
        }

        public int GetHashCode(CensusIndividual obj)
        {
            return obj.IndividualID.GetHashCode();
        }
    }
}
