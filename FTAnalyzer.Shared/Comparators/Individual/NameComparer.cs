using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class NameComparer : IComparer<IDisplayIndividual>
    {
        bool ForenamesFirst { get; }
        int Ascending { get; }

        public NameComparer(bool ascending, bool forenames)
        {
            ForenamesFirst = forenames;
            Ascending = ascending ? 1 : -1;
        }

        public int Compare(IDisplayIndividual x, IDisplayIndividual y)
        {
            if (ForenamesFirst)
            {
                if (x.Forenames.Equals(y.Forenames))
                {
                    if (x.Surname.Equals(y.Surname))
                        return Ascending * x.BirthDate.CompareTo(y.BirthDate);
                    return Ascending * string.Compare(x.Surname, y.Surname, StringComparison.Ordinal);
                }
                return Ascending * string.Compare(x.Forenames, y.Forenames, StringComparison.Ordinal);
            }
            if (x.Surname.Equals(y.Surname))
            {
                if (x.Forenames.Equals(y.Forenames))
                    return Ascending * x.BirthDate.CompareTo(y.BirthDate);
                return Ascending * string.Compare(x.Forenames, y.Forenames, StringComparison.Ordinal);
            }
            return Ascending * string.Compare(x.Surname, y.Surname, StringComparison.Ordinal);
        }
    }
}
