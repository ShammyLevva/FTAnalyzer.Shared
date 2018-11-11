using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class DefaultCensusComparer : Comparer<CensusIndividual>
    {
        public override int Compare(CensusIndividual x, CensusIndividual y)
        {
            int result = string.Compare(x.FamilyID, y.FamilyID, StringComparison.Ordinal);
            if (result == 0) result = x.Position - y.Position;
            return result;
        }
    }
}
