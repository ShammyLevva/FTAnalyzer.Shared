using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class ColourCensusComparer : Comparer<IDisplayColourCensus>
    {
        public override int Compare(IDisplayColourCensus x, IDisplayColourCensus y)
        {
            return x.Surname == y.Surname
                ? x.Forenames == y.Forenames
                    ? x.BirthDate.CompareTo(y.BirthDate)
                    : string.Compare(x.Forenames, y.Forenames, StringComparison.Ordinal)
                : string.Compare(x.Surname, y.Surname, StringComparison.Ordinal);
        }
    }
}
