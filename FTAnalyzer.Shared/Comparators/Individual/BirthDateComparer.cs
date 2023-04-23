using System.Collections.Generic;

namespace FTAnalyzer
{
    public class BirthDateComparer : Comparer<IDisplayIndividual>
    {
        public static readonly bool ASCENDING = true;
        public static readonly bool DESCENDING;

        public BirthDateComparer() : this(ASCENDING) { }

        public BirthDateComparer(bool direction) => Direction = direction;

        public bool Direction { get; set; } = ASCENDING;

        public override int Compare(IDisplayIndividual? x, IDisplayIndividual? y)
        {
            if (x is null && y is null) return 1;
            IDisplayIndividual? a = x, b=y;
            if(Direction == DESCENDING)
            {
                a = y;
                b = x;
            }
            if (a is null) return -1;
            if (b is null) return 1;
            return a.BirthDate.Equals(b.BirthDate)
                ? a.Surname.Equals(b.Surname)
                    ? string.Compare(a.Forenames, b.Forenames, System.StringComparison.Ordinal)
                    : string.Compare(a.Surname, b.Surname, System.StringComparison.Ordinal)
                : a.BirthDate.CompareTo(b.BirthDate);
        }
    }
}
