using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class NameComparer : Comparer<IDisplayIndividual>
    {
        public override int Compare(IDisplayIndividual x, IDisplayIndividual y)
        {
            return x.Surname.Equals(y.Surname)
                ? x.Forenames.Equals(y.Forenames)
                    ? x.BirthDate.CompareTo(y.BirthDate)
                    : string.Compare(x.Forenames, y.Forenames, StringComparison.Ordinal)
                : string.Compare(x.Surname, y.Surname, StringComparison.Ordinal);
        }
    }
}
