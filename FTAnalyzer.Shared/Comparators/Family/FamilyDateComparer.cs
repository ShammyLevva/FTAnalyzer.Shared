using System.Collections.Generic;

namespace FTAnalyzer
{
    class FamilyDateComparer : Comparer<Family>
    {
        public override int Compare(Family x, Family y)
        {
            return x.MarriageDate.CompareTo(y.MarriageDate);
        }
    }
}
