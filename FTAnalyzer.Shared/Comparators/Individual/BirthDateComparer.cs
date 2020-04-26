using System;
using System.Collections.Generic;

namespace FTAnalyzer
{
    public class BirthDateComparer : Comparer<IDisplayIndividual>
    {
        public static bool ASCENDING = true;
        public static bool DESCENDING;

        public BirthDateComparer() : this(ASCENDING) { }

        public BirthDateComparer(bool direction) => Direction = direction;

        public bool Direction { get; set; } = ASCENDING;

        public override int Compare(IDisplayIndividual x, IDisplayIndividual y)
        {
            if (x == null || y == null)
                return 0;
            IDisplayIndividual a = x, b=y;
            if(Direction == DESCENDING)
            {
                a = y;
                b = x;
            }
            return a.BirthDate.Equals(b.BirthDate, StringComparison.OrdinalIgnoreCase)
                ? a.Surname.Equals(b.Surname, StringComparison.OrdinalIgnoreCase)
                    ? string.Compare(a.Forenames, b.Forenames, StringComparison.OrdinalIgnoreCase)
                    : string.Compare(a.Surname, b.Surname, StringComparison.OrdinalIgnoreCase)
                : a.BirthDate.CompareTo(b.BirthDate);
        }
    }
}
