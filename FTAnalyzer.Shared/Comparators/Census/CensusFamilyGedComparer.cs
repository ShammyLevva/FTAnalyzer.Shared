using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTAnalyzer
{
    class CensusFamilyGedComparer : Comparer<CensusIndividual>
    {
        public override int Compare(CensusIndividual? x, CensusIndividual? y)
        {
            int r = string.Compare(x.FamilyID, y.FamilyID, StringComparison.Ordinal);
            if (r == 0)
            {
                r = x.Position - y.Position;
            }
            return r;
        }
    }
}
