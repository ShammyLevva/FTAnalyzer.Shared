using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class LooseDeathComparer : Comparer<IDisplayLooseDeath>
    {
        public override int Compare(IDisplayLooseDeath x, IDisplayLooseDeath y)
        {
            if (x == null || y == null)
                return 0;
            return x.Surname.Equals(y.Surname, StringComparison.OrdinalIgnoreCase)
                ? x.Forenames.Equals(y.Forenames, StringComparison.OrdinalIgnoreCase)
                    ? x.BirthDate.CompareTo(y.BirthDate)
                    : string.Compare(x.Forenames, y.Forenames, System.StringComparison.Ordinal)
                : string.Compare(x.Surname, y.Surname, System.StringComparison.Ordinal);
        }
    }
}
