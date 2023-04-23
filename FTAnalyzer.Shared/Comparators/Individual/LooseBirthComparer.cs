using System.Collections.Generic;

namespace FTAnalyzer
{
    public class LooseBirthComparer : Comparer<IDisplayLooseBirth>
    {
        public override int Compare(IDisplayLooseBirth? x, IDisplayLooseBirth? y)
        {
            return x.Surname.Equals(y.Surname)
                ? x.Forenames.Equals(y.Forenames)
                    ? x.BirthDate.CompareTo(y.BirthDate)
                    : string.Compare(x.Forenames, y.Forenames, System.StringComparison.Ordinal)
                : string.Compare(x.Surname, y.Surname, System.StringComparison.Ordinal);
        }
    }
}
