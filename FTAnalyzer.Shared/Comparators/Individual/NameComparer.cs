using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class NameComparer<T> : IComparer<T>
    {
        bool ForenamesFirst { get; }
        int Ascending { get; }

        public NameComparer(bool ascending, bool forenames)
        {
            ForenamesFirst = forenames;
            Ascending = ascending ? 1 : -1;
        }

        public int Compare(T? x, T? y)
        {
            var ind1 = x as Individual;
            var ind2 = y as Individual;

            if (ForenamesFirst)
            {
                if (ind1.Forenames.Equals(ind2.Forenames))
                {
                    if (ind1.Surname.Equals(ind2.Surname))
                        return Ascending * ind1.BirthDate.CompareTo(ind2.BirthDate);
                    return Ascending * string.Compare(ind1.Surname, ind2.Surname, StringComparison.Ordinal);
                }
                return Ascending * string.Compare(ind1.Forenames, ind2.Forenames, StringComparison.Ordinal);
            }
            if (ind1.Surname.Equals(ind2.Surname))
            {
                if (ind1.Forenames.Equals(ind2.Forenames))
                    return Ascending * ind1.BirthDate.CompareTo(ind2.BirthDate);
                return Ascending * string.Compare(ind1.Forenames, ind2.Forenames, StringComparison.Ordinal);
            }
            return Ascending * string.Compare(ind1.Surname, ind2.Surname, StringComparison.Ordinal);
        }
    }
}
