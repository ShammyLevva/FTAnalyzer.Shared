using System;

namespace FTAnalyzer
{
    class CensusIndividualNameComparer : DefaultCensusComparer
    {
        public override int Compare(CensusIndividual c1, CensusIndividual c2)
        {
            int r = string.Compare(c1.CensusSurname, c2.CensusSurname, StringComparison.Ordinal);
            if (r == 0) r = base.Compare(c1, c2);
            return r;
        }
    }
}
