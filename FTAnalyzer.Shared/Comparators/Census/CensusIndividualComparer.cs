using System;
using System.Collections.Generic;
using System.Text;

namespace FTAnalyzer
{
    class CensusIndividualComparer : IEqualityComparer<CensusIndividual>
    {
        public bool Equals(CensusIndividual x, CensusIndividual y)
        {
            return x.IndividualID.Equals(y.IndividualID);
        }

        public int GetHashCode(CensusIndividual obj)
        {
            return obj.IndividualID.GetHashCode();
        }
    }
}
