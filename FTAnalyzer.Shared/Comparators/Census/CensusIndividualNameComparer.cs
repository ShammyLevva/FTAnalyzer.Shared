using System;

namespace FTAnalyzer
{
    class CensusIndividualNameComparer : DefaultCensusComparer
    {
        public override int Compare(CensusIndividual x, CensusIndividual y)
        {
            int r = string.Compare(x.CensusSurname, y.CensusSurname, StringComparison.Ordinal);
            if (r == 0) r = base.Compare(x, y);
            return r;
        }
    }
}
