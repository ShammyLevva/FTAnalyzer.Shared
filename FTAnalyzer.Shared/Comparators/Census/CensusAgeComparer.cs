using System.Collections.Generic;

namespace FTAnalyzer
{
    public class CensusAgeComparer : IComparer<CensusIndividual>
    {
        public int Compare(CensusIndividual i1, CensusIndividual i2)
        {
            if (i1.CensusStatus == i2.CensusStatus)
                // same status so sort by date
                return SortBirthdate(i1, i2);
            if (i1.CensusStatus == CensusIndividual.HUSBAND)
                return -1;
            if (i2.CensusStatus == CensusIndividual.HUSBAND)
                return 1;
            // neither is husband so is one a wife
            if (i1.CensusStatus == CensusIndividual.WIFE)
                return -1;
            if (i2.CensusStatus == CensusIndividual.WIFE)
                return 1;
            // neither is husband or wife so is one a child
            if (i1.CensusStatus == CensusIndividual.CHILD)
                return -1;
            if (i2.CensusStatus == CensusIndividual.CHILD)
                return 1;
            return 0;
        }

        static int SortBirthdate(Individual i1, Individual i2)
        {
            Fact b1 = i1.BirthFact;
            Fact b2 = i2.BirthFact;
            FactDate d1 = (b1 is null) ? FactDate.UNKNOWN_DATE : b1.FactDate;
            FactDate d2 = (b2 is null) ? FactDate.UNKNOWN_DATE : b2.FactDate;
            return d1.CompareTo(d2);
        }
    }
}