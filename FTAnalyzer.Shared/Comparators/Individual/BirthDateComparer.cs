using System.Collections.Generic;

namespace FTAnalyzer
{
    public class BirthDateComparer : Comparer<IDisplayIndividual>
    {
        public static bool ASCENDING = true;
        public static bool DESCENDING = false;

        public BirthDateComparer() : this(ASCENDING) { }

        public BirthDateComparer(bool direction) => Direction = direction;

        public bool Direction { get; set; } = ASCENDING;

        public override int Compare(IDisplayIndividual x, IDisplayIndividual y)
        {
            IDisplayIndividual a = x, b=y;
            if(Direction == DESCENDING)
            {
                a = y;
                b = x;
            }
            if (a.BirthDate.Equals(b.BirthDate))
            {
                if (a.Surname.Equals(b.Surname))
                    return a.Forenames.CompareTo(b.Forenames);
                else
                    return a.Surname.CompareTo(b.Surname);
            }
            else
                return a.BirthDate.CompareTo(b.BirthDate);
        }
    }
}
